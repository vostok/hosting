using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.ClusterConfig.Client;
using Vostok.Commons.Helpers.Extensions;
using Vostok.Commons.Helpers.Observable;
using Vostok.Commons.Threading;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Components.Environment;
using Vostok.Hosting.Models;
using Vostok.Logging.Abstractions;
using Vostok.Tracing.Abstractions;

namespace Vostok.Hosting
{
    [PublicAPI]
    public class VostokHost
    {
        public readonly CancellationTokenSource ShutdownTokenSource;

        private readonly VostokHostSettings settings;
        private readonly CachingObservable<VostokApplicationState> onApplicationStateChanged;
        private readonly AtomicBoolean launchedOnce = false;
        private VostokHostingEnvironment environment;
        private ILog log;

        public VostokHost([NotNull] VostokHostSettings settings)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));

            ShutdownTokenSource = new CancellationTokenSource();

            onApplicationStateChanged = new CachingObservable<VostokApplicationState>();
            ChangeStateTo(VostokApplicationState.NotInitialized);
        }

        public VostokApplicationState ApplicationState { get; private set; }

        public IObservable<VostokApplicationState> OnApplicationStateChanged => onApplicationStateChanged;

        public async Task<VostokApplicationRunResult> RunAsync()
        {
            if (!launchedOnce.TrySetTrue())
                throw new InvalidOperationException("Application can't be launched multiple times.");

            if (settings.ConfigureThreadPool)
                ThreadPoolUtility.Setup();

            using (environment = EnvironmentBuilder.Build(settings.EnvironmentSetup, ShutdownTokenSource.Token))
            {
                log = environment.Log.ForContext<VostokHost>();

                LogApplicationIdentity(environment.ApplicationIdentity);

                ConfigureHostBeforeRun();

                var result = await InitializeApplicationAsync().ConfigureAwait(false);
                if (result.State == VostokApplicationState.Initialized)
                    result = await RunApplicationAsync().ConfigureAwait(false);
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
                var task = initialize
                    ? settings.Application.InitializeAsync(environment)
                    : settings.Application.RunAsync(environment);

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

            if (environment.ClusterConfigClient is ClusterConfigClient clusterConfigClient)
            {
                if (!ClusterConfigClient.TrySetDefaultClient(clusterConfigClient))
                    log.Warn("ClusterConfigClient.Default has been already configured.");
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