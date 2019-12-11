using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.ClusterConfig.Client;
using Vostok.Commons.Helpers.Extensions;
using Vostok.Commons.Helpers.Observable;
using Vostok.Commons.Threading;
using Vostok.Configuration;
using Vostok.Datacenters;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Components.Environment;
using Vostok.Hosting.Models;
using Vostok.Logging.Abstractions;
using Vostok.Tracing.Abstractions;

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

        private readonly VostokHostSettings settings;
        private readonly CachingObservable<VostokApplicationState> onApplicationStateChanged;
        private readonly AtomicBoolean launchedOnce = false;

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
        public VostokApplicationState ApplicationState { get; private set; }

        /// <summary>
        /// <para>Returns an observable sequence of application states.</para>
        /// <para>This sequence produces <see cref="IObserver{T}.OnNext"/> notifications every time current <see cref="ApplicationState"/> changes.</para>
        /// <para>This sequence produces <see cref="IObserver{T}.OnError"/> if application crashes.</para>
        /// <para>This sequence produces <see cref="IObserver{T}.OnCompleted"/> notification when application execution completes.</para>
        /// <para>Immediately produces a notification with current <see cref="ApplicationState"/> when subscribed to.</para>
        /// </summary>
        public IObservable<VostokApplicationState> OnApplicationStateChanged => onApplicationStateChanged;

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
        public async Task<VostokApplicationRunResult> RunAsync()
        {
            if (!launchedOnce.TrySetTrue())
                throw new InvalidOperationException("Application can't be launched multiple times.");

            if (settings.ConfigureThreadPool)
                ThreadPoolUtility.Setup();

            using (environment = EnvironmentBuilder.Build(settings.EnvironmentSetup, ShutdownTokenSource.Token, settings.Application.GetType()))
            {
                log = environment.Log.ForContext<VostokHost>();

                LogApplicationIdentity(environment.ApplicationIdentity);

                ConfigureHostBeforeRun();

                var result = await InitializeApplicationAsync().ConfigureAwait(false);
                if (result.State == VostokApplicationState.Initialized)
                    result = await RunApplicationAsync().ConfigureAwait(false);

                onApplicationStateChanged.Complete();

                return result;
            }
        }

        private async Task<VostokApplicationRunResult> InitializeApplicationAsync()
        {
            log.Info("Initializing application.");
            ChangeStateTo(VostokApplicationState.Initializing);

            try
            {
                return await RunPhaseAsync(true).ConfigureAwait(false);
            }
            catch (Exception error)
            {
                log.Error(error, "Unhandled exception has occurred while initializing application.");
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
                log.Error(error, "Unhandled exception has occurred while running application.");
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
                        log.Error(error, "Unhandled exception has occurred while stopping application.");
                        return ReturnResult(VostokApplicationState.CrashedDuringStopping, error);
                    }

                    log.Info("Application successfully stopped.");
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
        }

        private void ConfigureHostBeforeRun()
        {
            var cpuUnitsLimit = environment.ApplicationLimits.CpuUnits;
            if (settings.ConfigureThreadPool && cpuUnitsLimit.HasValue)
                ThreadPoolUtility.Setup(processorCount: cpuUnitsLimit.Value);

            if (settings.ConfigureStaticProviders)
                ConfigureStaticProviders();
        }

        private void ConfigureStaticProviders()
        {
            LogProvider.Configure(environment.Log, true);
            TracerProvider.Configure(environment.Tracer, true);
            HerculesSinkProvider.Configure(environment.HerculesSink, true);
            DatacentersProvider.Configure(environment.Datacenters, true);

            if (environment.ClusterConfigClient is ClusterConfigClient clusterConfigClient)
            {
                if (!ClusterConfigClient.TrySetDefaultClient(clusterConfigClient))
                    log.Warn("ClusterConfigClient.Default has already been configured.");
            }

            if (environment.ConfigurationProvider is ConfigurationProvider configurationProvider)
            {
                if (!ConfigurationProvider.TrySetDefault(configurationProvider))
                    log.Warn("ConfigurationProvider.Default has already been configured.");
            }
        }

        #region Logging

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

        #endregion
    }
}