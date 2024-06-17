using System;
using System.Diagnostics;
#if NET6_0_OR_GREATER
using System.Runtime.InteropServices;
#endif
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Commons.Helpers.Extensions;
using Vostok.Commons.Helpers.Observable;
using Vostok.Commons.Threading;
using Vostok.Commons.Time;
using Vostok.Configuration.Abstractions.Extensions.Observable;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Helpers;
using Vostok.Hosting.Abstractions.Requirements;
using Vostok.Hosting.Components.Diagnostics;
using Vostok.Hosting.Components.Environment;
using Vostok.Hosting.Components.Metrics;
using Vostok.Hosting.Components.ThreadPool;
using Vostok.Hosting.Helpers;
using Vostok.Hosting.Models;
using Vostok.Hosting.Requirements;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.ServiceDiscovery;
using Vostok.ServiceDiscovery.Abstractions;

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
        /// The cancellation token source that should be used to stop the application.
        /// </summary>
        public readonly CancellationTokenSource ShutdownTokenSource;

        protected readonly VostokHostSettings settings;

        private readonly CachingObservable<VostokApplicationState> onApplicationStateChanged;
        private readonly AtomicBoolean launchedOnce = false;
        private readonly object launchGate = new object();

        private volatile Task<VostokApplicationRunResult> workerTask;
        private volatile VostokHostingEnvironment environment;
        private volatile ILog log;
#if NET6_0_OR_GREATER
        private volatile PosixSignalRegistration sigtermRegistration;
#endif

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
        /// <para>Note that terminal notifications (<see cref="IObserver{T}.OnError"/> and <see cref="IObserver{T}.OnCompleted"/>)
        /// do not guarantee full completion of the task returned by <see cref="RunAsync"/> (the host may run its own cleanup after those).
        /// These notifications merely signify the final, terminal nature of the last reported status.</para>
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
        /// <para>Starts the execution of the application and optionally waits for given state to occur.</para>
        /// <para>If not given a <paramref name="stateToAwait"/>, acts in a fire-and-forget fashion.</para>
        /// <para>If given <paramref name="stateToAwait"/> is not reached before the task returned by
        /// <see cref="RunAsync"/> completes, simply awaits that task instead, propagating its error in case of crash.</para>
        /// <para>Waits for the <see cref="VostokApplicationState.Running"/> state by default.</para>
        /// </summary>
        public async Task StartAsync(VostokApplicationState? stateToAwait = VostokApplicationState.Running)
        {
            var stateCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            var subscription = OnApplicationStateChanged.Subscribe(
                state =>
                {
                    if (state == stateToAwait)
                        stateCompletionSource.TrySetResult(true);
                });

            using (subscription)
            {
                var runnerTask = RunAsync().ContinueWith(task => task.Result.EnsureSuccess(), TaskContinuationOptions.OnlyOnRanToCompletion);

                if (stateToAwait == null)
                    return;

                var completedTask = await Task.WhenAny(runnerTask, stateCompletionSource.Task).ConfigureAwait(false);

                await completedTask.ConfigureAwait(false);
            }
        }

        /// <summary>
        /// <para>Cancels the execution of the application and waits for the host to stop.</para>
        /// <para>If <paramref name="ensureSuccess"/> is <c>true</c> (which is the default), propagates errors from app crashes.</para>
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
        
#if NET6_0_OR_GREATER
        /// <summary>
        /// Listen <see cref="PosixSignal.SIGTERM"/> and shutdown VostokHost if received.
        /// </summary>
#else
        /// <summary>
        /// Listen <see cref="AppDomain.ProcessExit"/> and shutdown VostokHost if SIGTERM received.
        /// </summary>
#endif
        public VostokHost RegisterSigtermCancellation()
        {
#if NET6_0_OR_GREATER
            sigtermRegistration = PosixSignalRegistration.Create(PosixSignal.SIGTERM, ctx =>
            {
                ctx.Cancel = true;
                this.Stop(false);
            });
#else
            AppDomain.CurrentDomain.ProcessExit += (_, _) => this.Stop(false);
#endif
            return this;
        }

        private async Task<VostokApplicationRunResult> RunInternalAsync()
        {
            if (settings.ConfigureThreadPool)
                ThreadPoolUtility.Setup(settings.ThreadPoolTuningMultiplier);

            var result = BuildEnvironment();
            if (result != null)
                return result;

            var dynamicThreadPool = ConfigureDynamicThreadPool();

            using (environment)
            using (new ApplicationDisposable(settings.Application, environment, log))
            using (dynamicThreadPool)
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
                    ConfigureStaticProviders = settings.ConfigureStaticProviders,
                    BeaconShutdownTimeout = settings.BeaconShutdownTimeout,
                    BeaconShutdownWaitEnabled = settings.BeaconShutdownWaitEnabled,
                    DisposeComponentTimeout = settings.DisposeComponentTimeout,
                    SendAnnotations = settings.SendAnnotations,
                    DiagnosticMetricsEnabled = settings.DiagnosticMetricsEnabled
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
            builder.SetupHostExtensions(
                extensions =>
                {
                    var vostokHostShutdown = new VostokHostShutdown(ShutdownTokenSource);
                    extensions.Add(vostokHostShutdown);
                    extensions.Add(typeof(IVostokHostShutdown), vostokHostShutdown);
                });

            RequirementsHelper.EnsurePort(settings.Application, builder);
            RequirementsHelper.EnsureConfigurations(settings.Application, builder);

            settings.EnvironmentSetup(builder);
        }

        [CanBeNull]
        private VostokApplicationRunResult WarmupEnvironment()
        {
            ChangeStateTo(VostokApplicationState.EnvironmentWarmup);

            try
            {
                ConfigureHostBeforeRun();

                environment.Warmup(new VostokHostingEnvironmentWarmupSettings
                {
                    LogApplicationConfiguration = settings.LogApplicationConfiguration,
                    LogDotnetEnvironmentVariables = settings.LogDotnetEnvironmentVariables,
                    WarmupConfiguration = settings.WarmupConfiguration,
                    WarmupZooKeeper = settings.WarmupZooKeeper
                });

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
                RequirementsChecker.Check(settings.Application, environment);

                var initializationResult = await RunPhaseAsync(true).ConfigureAwait(false);
                if (initializationResult.State != VostokApplicationState.Initialized)
                    return initializationResult;

                environment.ServiceBeacon.Start();
                var beaconStarted = await WaitForServiceBeaconRegistrationIfNeededAsync(environment.ServiceBeacon).ConfigureAwait(false);
                if (!beaconStarted)
                    return ReturnResult(VostokApplicationState.CrashedDuringInitialization, new Exception($"Service beacon hasn't registered in '{settings.BeaconRegistrationTimeout}'."));

                if (settings.DiagnosticMetricsEnabled)
                    HealthTrackerHelper.LaunchPeriodicalChecks(environment.Diagnostics, environment.ShutdownToken);

                return initializationResult;
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
                if (settings.SendAnnotations)
                    AnnotationsHelper.ReportInitialized(environment.ApplicationIdentity, environment.Metrics.Instance);

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
            var applicationTask = initialize
                ? Task.Run(async () => await settings.Application.InitializeAsync(environment).ConfigureAwait(false))
                : Task.Run(async () => await settings.Application.RunAsync(environment).ConfigureAwait(false));

            await Task.WhenAny(applicationTask, environment.ShutdownTask).ConfigureAwait(false);

            if (!initialize)
                environment.ServiceBeacon.Stop();

            if (environment.ShutdownTask.IsCompleted)
            {
                var watch = Stopwatch.StartNew();
                ChangeStateTo(VostokApplicationState.Stopping);

                if (!await applicationTask.TryWaitAsync(environment.ShutdownTimeout).ConfigureAwait(false))
                {
                    LogApplicationHasNotCompletedWithinTimeout();
                    return ReturnResult(VostokApplicationState.StoppedForcibly);
                }

                try
                {
                    await applicationTask.ConfigureAwait(false);
                }
                catch (Exception error)
                {
                    if (error is OperationCanceledException)
                    {
                        LogApplicationSuccessfullyStopped(watch.Elapsed);
                        return ReturnResult(VostokApplicationState.Stopped);
                    }

                    log.Error(error, "Unhandled exception has occurred while stopping application.");
                    return ReturnResult(VostokApplicationState.CrashedDuringStopping, error);
                }

                LogApplicationSuccessfullyStopped(watch.Elapsed);
                return ReturnResult(VostokApplicationState.Stopped);
            }

            await applicationTask.ConfigureAwait(false);

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

        private async Task<bool> WaitForServiceBeaconRegistrationIfNeededAsync(IServiceBeacon beacon)
        {
            if (!RequirementDetector.RequiresPort(settings.Application) || !settings.BeaconRegistrationWaitEnabled || !(beacon is ServiceBeacon convertedBeacon))
                return true;

            return await convertedBeacon.WaitForInitialRegistrationAsync()
                .TryWaitAsync(settings.BeaconRegistrationTimeout)
                .ConfigureAwait(false);
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
        }

        private DynamicThreadPoolTracker ConfigureDynamicThreadPool()
        {
            if (settings.ThreadPoolSettingsProvider == null)
                return null;

            var dynamicThreadPoolTracker = new DynamicThreadPoolTracker(
                () => settings.ThreadPoolSettingsProvider(environment.ConfigurationProvider),
                environment.ApplicationLimits,
                environment.Log);

            return dynamicThreadPoolTracker;
        }

        #region Logging

        private void LogApplicationHasNotCompletedWithinTimeout() =>
            log.Warn("Application has not completed within remaining shutdown timeout.");

        private void LogApplicationSuccessfullyStopped(TimeSpan elapsed) =>
            log.Info("Application has successfully stopped in {ApplicationStopTime}.", elapsed.ToPrettyString());

        #endregion
    }
}