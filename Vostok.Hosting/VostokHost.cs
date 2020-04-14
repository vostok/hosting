﻿using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Commons.Environment;
using Vostok.Commons.Helpers.Extensions;
using Vostok.Commons.Helpers.Observable;
using Vostok.Commons.Threading;
using Vostok.Configuration.Abstractions.Extensions.Observable;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Extensions;
using Vostok.Configuration.Primitives;
using Vostok.Datacenters;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Requirements;
using Vostok.Hosting.Components.Environment;
using Vostok.Hosting.Helpers;
using Vostok.Hosting.Models;
using Vostok.Hosting.Requirements;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.ZooKeeper.Client.Abstractions;
// ReSharper disable SuspiciousTypeConversion.Global

namespace Vostok.Hosting
{
    /// <summary>
    /// <para>An <see cref="IVostokApplication"/> launcher.</para>
    /// <para>May be used as self-hosted or inside some hosting like kubernetes.</para>
    /// <para>Responsible for doing the following:</para>
    /// <list type="bullet">
    ///     <item><description>Creating an instance of <see cref="IVostokHostingEnvironment"/> using <see cref="VostokHostSettings.EnvironmentSetup"/>.</description></item>
    ///     <item><description>Running the application by calling <see cref="IVostokApplication.InitializeAsync"/> and then <see cref="IVostokApplication.RunAsync"/>.</description></item>
    /// </list>
    /// </summary>
    [PublicAPI]
    public class VostokHost
    {
        /// <summary>
        /// A cancellation token source which should be used for stopping application.
        /// </summary>
        public readonly CancellationTokenSource ShutdownTokenSource;

        protected readonly VostokHostSettings settings;

        private readonly CachingObservable<VostokApplicationState> onApplicationStateChanged;
        private readonly AtomicBoolean launchedOnce = false;
        private readonly object launchGate = new object();

        private volatile Task<VostokApplicationRunResult> workerTask;
        private volatile VostokHostingEnvironment environment;
        private volatile ILog log;

        public VostokHost([NotNull] VostokHostSettings settings)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));

            ShutdownTokenSource = new CancellationTokenSource();

            onApplicationStateChanged = new CachingObservable<VostokApplicationState>();
            ChangeStateTo(VostokApplicationState.NotInitialized);
        }

        /// <summary>
        /// Returns current <see cref="VostokApplicationState"/>.
        /// </summary>
        public virtual VostokApplicationState ApplicationState { get; private set; }

        /// <summary>
        /// <para>Returns an observable sequence of application states.</para>
        /// <para>This sequence produces <see cref="IObserver{T}.OnNext"/> notifications every time current <see cref="ApplicationState"/> changes.</para>
        /// <para>This sequence produces <see cref="IObserver{T}.OnError"/> if application crashes.</para>
        /// <para>This sequence produces <see cref="IObserver{T}.OnCompleted"/> notification when application execution completes.</para>
        /// <para>Immediately produces a notification with current <see cref="ApplicationState"/> when subscribed to.</para>
        /// </summary>
        public virtual IObservable<VostokApplicationState> OnApplicationStateChanged => onApplicationStateChanged;

        /// <summary>
        /// <para>Launches the provided <see cref="IVostokApplication"/>.</para>
        /// <para>Performs following operations:</para>
        /// <list type="bullet">
        ///     <item><description>Creates an instance of <see cref="IVostokHostingEnvironment"/> using <see cref="VostokHostSettings.EnvironmentSetup"/>.</description></item>
        ///     <item><description>Configures thread pool if <see cref="VostokHostSettings.ConfigureThreadPool"/> specified.</description></item>
        ///     <item><description>Configures static providers if <see cref="VostokHostSettings.ConfigureStaticProviders"/> specified.</description></item>
        ///     <item><description>Calls <see cref="IVostokApplication.InitializeAsync"/>.</description></item>
        ///     <item><description>Calls <see cref="IVostokApplication.RunAsync"/>.</description></item>
        ///     <item><description>Stops application if <see cref="ShutdownTokenSource"/> has been canceled.</description></item>
        /// </list>
        /// <para>May throw an exception if an error occurs during environment creation.</para>
        /// <para>Does not rethrow exceptions from <see cref="IVostokApplication"/>, stores them in result's <see cref="VostokApplicationRunResult.Error"/> property.</para>
        /// </summary>
        public virtual Task<VostokApplicationRunResult> RunAsync()
        {
            lock (launchGate)
            {
                if (!launchedOnce.TrySetTrue())
                    throw new InvalidOperationException("Application can't be launched multiple times.");

                workerTask = Task.Run(RunInternalAsync);
            }

            return workerTask;
        }

        /// <summary>
        /// Starts the execution of the application and optionally waits for given state to occur.
        /// </summary>
        public async Task StartAsync(VostokApplicationState? stateToAwait = null)
        {
            var runnerTask = RunAsync().ContinueWith(task => task.Result.EnsureSuccess(), TaskContinuationOptions.OnlyOnRanToCompletion);

            if (stateToAwait == null)
                return;

            var stateCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            var subscription = OnApplicationStateChanged.Subscribe(
                state =>
                {
                    if (state == stateToAwait)
                        stateCompletionSource.TrySetResult(true);
                });

            using (subscription)
            {
                var completedTask = await Task.WhenAny(runnerTask, stateCompletionSource.Task).ConfigureAwait(false);

                await completedTask.ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Cancels the execution of the application and waits for the host to stop.
        /// </summary>
        public Task<VostokApplicationRunResult> StopAsync(bool ensureSuccess = true)
        {
            Task<VostokApplicationRunResult> resultTask;

            lock (launchGate)
                resultTask = workerTask;

            if (resultTask == null)
                return Task.FromResult(new VostokApplicationRunResult(VostokApplicationState.NotInitialized));

            ShutdownTokenSource.Cancel();

            if (ensureSuccess)
                resultTask = resultTask.ContinueWith(task => task.Result.EnsureSuccess(), TaskContinuationOptions.OnlyOnRanToCompletion);

            return resultTask;
        }

        private async Task<VostokApplicationRunResult> RunInternalAsync()
        {
            if (settings.ConfigureThreadPool)
                ThreadPoolUtility.Setup(settings.ThreadPoolTuningMultiplier);

            var result = BuildEnvironment();
            if (result != null)
                return result;

            using (environment)
            using (settings.Application as IDisposable)
            {
                result = WarmupEnvironment();

                if (result != null)
                    return result;

                result = await InitializeApplicationAsync().ConfigureAwait(false);

                if (result.State == VostokApplicationState.Initialized)
                    result = await RunApplicationAsync().ConfigureAwait(false);

                return result;
            }
        }

        [CanBeNull]
        private VostokApplicationRunResult BuildEnvironment()
        {
            ChangeStateTo(VostokApplicationState.EnvironmentSetup);

            try
            {
                var environmentFactorySettings = new VostokHostingEnvironmentFactorySettings
                {
                    ConfigureStaticProviders = settings.ConfigureStaticProviders
                };

                environment = EnvironmentBuilder.Build(SetupEnvironment, environmentFactorySettings);
                
                log = environment.Log.ForContext<VostokHost>();
                
                return null;
            }
            catch (Exception error)
            {
                return ReturnResult(VostokApplicationState.CrashedDuringEnvironmentSetup, error);
            }
        }

        private void SetupEnvironment(IVostokHostingEnvironmentBuilder builder)
        {
            builder.SetupShutdownToken(ShutdownTokenSource.Token);
            builder.SetupShutdownTimeout(settings.ShutdownTimeout);

            var applicationType = settings.Application.GetType();

            RequirementsHelper.EnsurePort(applicationType, builder);
            RequirementsHelper.EnsureConfigurations(applicationType, builder);

            settings.EnvironmentSetup(builder);
        }

        [CanBeNull]
        private VostokApplicationRunResult WarmupEnvironment()
        {
            ChangeStateTo(VostokApplicationState.EnvironmentWarmup);

            try
            {
                LogEnvironmentInfo();
                LogApplicationIdentity(environment.ApplicationIdentity);
                LogLocalDatacenter(environment.Datacenters);
                LogApplicationLimits(environment.ApplicationLimits);
                LogApplicationReplication(environment.ApplicationReplicationInfo);
                LogHostExtensions(environment.HostExtensions);

                ConfigureHostBeforeRun();
                LogThreadPoolSettings();

                WarmupConfiguration();
                WarmupZooKeeper();

                foreach (var action in settings.BeforeInitializeApplication)
                    action(environment);

                return null;
            }
            catch (Exception error)
            {
                log.Error(error, "Unhandled exception has occurred while warming up the environment.");
                return ReturnResult(VostokApplicationState.CrashedDuringEnvironmentWarmup, error);
            }
        }

        private async Task<VostokApplicationRunResult> InitializeApplicationAsync()
        {
            log.Info("Initializing application.");
            ChangeStateTo(VostokApplicationState.Initializing);

            try
            {
                RequirementsChecker.Check(settings.Application.GetType(), environment);

                return await RunPhaseAsync(true).ConfigureAwait(false);
            }
            catch (Exception error)
            {
                log.Error(error, "Unhandled exception has occurred while initializing the application.");
                return ReturnResult(VostokApplicationState.CrashedDuringInitialization, error);
            }
        }

        private async Task<VostokApplicationRunResult> RunApplicationAsync()
        {
            log.Info("Running application.");
            ChangeStateTo(VostokApplicationState.Running);

            try
            {
                return await RunPhaseAsync(false).ConfigureAwait(false);
            }
            catch (Exception error)
            {
                log.Error(error, "Unhandled exception has occurred while running the application.");
                return ReturnResult(VostokApplicationState.CrashedDuringRunning, error);
            }
        }

        private async Task<VostokApplicationRunResult> RunPhaseAsync(bool initialize)
        {
            var shutdown = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var shutdownToken = environment.ShutdownToken;

            using (shutdownToken.Register(o => ((TaskCompletionSource<bool>)o).TrySetCanceled(), shutdown))
            {
                // ReSharper disable MethodSupportsCancellation
                // Note(kungurtsev): Task.Run needed for synchronous code.
                var task = initialize
                    ? Task.Run(async () => await settings.Application.InitializeAsync(environment).ConfigureAwait(false))
                    : Task.Run(async () => await settings.Application.RunAsync(environment).ConfigureAwait(false));
                // ReSharper restore MethodSupportsCancellation

                if (!initialize)
                    environment.ServiceBeacon.Start();

                await Task.WhenAny(task, shutdown.Task).ConfigureAwait(false);

                if (!initialize)
                    environment.ServiceBeacon.Stop();

                if (shutdownToken.IsCancellationRequested)
                {
                    log.Info("Cancellation requested, waiting for application to complete within timeout = {Timeout}.", settings.ShutdownTimeout);
                    ChangeStateTo(VostokApplicationState.Stopping);

                    if (!await task.WaitAsync(settings.ShutdownTimeout).ConfigureAwait(false))
                    {
                        log.Info("Cancellation requested, but application has not exited within {Timeout} timeout.", settings.ShutdownTimeout);
                        return ReturnResult(VostokApplicationState.StoppedForcibly);
                    }

                    try
                    {
                        await task.ConfigureAwait(false);
                    }
                    catch (Exception error)
                    {
                        if (error is OperationCanceledException)
                        {
                            log.Info("Application has successfully stopped.");
                            return ReturnResult(VostokApplicationState.Stopped);
                        }

                        log.Error(error, "Unhandled exception has occurred while stopping application.");
                        return ReturnResult(VostokApplicationState.CrashedDuringStopping, error);
                    }

                    log.Info("Application has successfully stopped.");
                    return ReturnResult(VostokApplicationState.Stopped);
                }

                await task.ConfigureAwait(false);

                if (initialize)
                {
                    log.Info("Application initialization completed successfully.");
                    return ReturnResult(VostokApplicationState.Initialized);
                }
                // ReSharper disable once RedundantIfElseBlock
                else
                {
                    log.Info("Application exited.");
                    return ReturnResult(VostokApplicationState.Exited);
                }
            }
        }

        private VostokApplicationRunResult ReturnResult(VostokApplicationState newState, Exception error = null)
        {
            ChangeStateTo(newState, error);
            return new VostokApplicationRunResult(newState, error);
        }

        private void ChangeStateTo(VostokApplicationState newState, Exception error = null)
        {
            ApplicationState = newState;
            
            onApplicationStateChanged.Next(newState);
            
            if (error != null)
                onApplicationStateChanged.Error(error);
            else if (newState.IsTerminal())
                onApplicationStateChanged.Complete();
        }

        private void ConfigureHostBeforeRun()
        {
            var cpuUnitsLimit = environment.ApplicationLimits.CpuUnits;
            if (settings.ConfigureThreadPool && cpuUnitsLimit.HasValue)
                ThreadPoolUtility.Setup(settings.ThreadPoolTuningMultiplier, cpuUnitsLimit.Value);

            if (settings.ConfigureStaticProviders)
                StaticProvidersHelper.Configure(environment);
        }

        private void WarmupConfiguration()
        {
            if (!settings.WarmupConfiguration)
                return;

            log.Info("Warming up application configuration..");

            environment.ClusterConfigClient.Get(Guid.NewGuid().ToString());

            var ordinarySettings = environment.ConfigurationSource.Get();

            environment.SecretConfigurationSource.Get();

            if (settings.LogApplicationConfiguration)
                LogApplicationConfiguration(ordinarySettings);
        }

        private void WarmupZooKeeper()
        {
            if (settings.WarmupZooKeeper && environment.HostExtensions.TryGet<IZooKeeperClient>(out var zooKeeperClient))
            {
                log.Info("Warming up ZooKeeper connection..");

                zooKeeperClient.Exists("/");
            }
        }

        #region Logging

        private void LogEnvironmentInfo()
        {
            log.Info("Application user = '{User}'.", Environment.UserName);
            log.Info("Application host = '{Host}'.", EnvironmentInfo.Host);
            log.Info("Application host FQDN = '{HostFQDN}'.", EnvironmentInfo.FQDN);
            log.Info("Application process id = '{ProcessId}'.", EnvironmentInfo.ProcessId);
            log.Info("Application process name = '{ProcessName}'.", EnvironmentInfo.ProcessName);
            log.Info("Application base directory = '{BaseDirectory}'.", EnvironmentInfo.BaseDirectory);
            log.Info("Application current directory = '{CurrentDirectory}'.", Environment.CurrentDirectory);
            log.Info("Application OS = '{OperatingSystem}'.", RuntimeInformation.OSDescription);
            log.Info("Application bitness = '{Bitness}'.", Environment.Is64BitProcess ? "x64" : "x86");
            log.Info("Application framework = '{Framework}'.", RuntimeInformation.FrameworkDescription);
        }

        private void LogApplicationIdentity(IVostokApplicationIdentity applicationIdentity)
        {
            var messageTemplate = applicationIdentity.Subproject == null
                ? "Application identity: project: '{Project}', environment: '{Environment}', application: '{Application}', instance: '{Instance}'."
                : "Application identity: project: '{Project}', subproject: '{Subproject}', environment: '{Environment}', application: '{Application}', instance: '{Instance}'.";

            var messageParameters = applicationIdentity.Subproject == null
                ? new object[] {applicationIdentity.Project, applicationIdentity.Environment, applicationIdentity.Application, applicationIdentity.Instance}
                : new object[] {applicationIdentity.Project, applicationIdentity.Subproject, applicationIdentity.Environment, applicationIdentity.Application, applicationIdentity.Instance};

            log.Info(messageTemplate, messageParameters);
        }

        private void LogLocalDatacenter(IDatacenters datacenters) =>
            log.Info("Application datacenter: {DatacenterName}.", datacenters.GetLocalDatacenter() ?? "unknown");

        private void LogApplicationLimits(IVostokApplicationLimits limits) =>
            log.Info(
                "Application limits: {CpuLimit} CPU, {MemoryLimit} memory.",
                limits.CpuUnits?.ToString("F2") ?? "unlimited",
                limits.MemoryBytes.HasValue ? new DataSize(limits.MemoryBytes.Value).ToString() : "unlimited");

        private void LogApplicationReplication(IVostokApplicationReplicationInfo info) =>
            log.Info("Application replication: instance {InstanceIndex} of {InstanceCount}.", info.InstanceIndex, info.InstancesCount);

        private void LogApplicationConfiguration(ISettingsNode configuration)
        {
            try
            {
                log.Info($"Application configuration: {Environment.NewLine}{{ApplicationConfiguration}}.", configuration);
            }
            catch
            {
                log.Warn("Application configuration is unknown.");
            }
        }

        private void LogHostExtensions(IVostokHostExtensions extensions)
            => log.Info("Registered host extensions: {HostExtensions}.", extensions.GetAll().Select(pair => pair.Item1.Name).ToArray());

        private void LogThreadPoolSettings()
        {
            var state = ThreadPoolUtility.GetPoolState();

            log.Info("Thread pool configuration: {MinWorkerThreads} min workers, {MinIOCPThreads} min IOCP.", state.MinWorkerThreads, state.MinIocpThreads);
        }

        #endregion
    }
}