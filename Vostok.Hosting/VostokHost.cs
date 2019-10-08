using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Commons.Helpers.Extensions;
using Vostok.Commons.Helpers.Observable;
using Vostok.Commons.Threading;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Components.Environment;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting
{
    [PublicAPI]
    public class VostokHost
    {
        public readonly CancellationTokenSource ShutdownTokenSource;
        public volatile VostokApplicationState ApplicationState;

        private readonly VostokHostSettings settings;
        private readonly CachingObservable<VostokApplicationState> onApplicationStateChanged;
        private readonly IVostokApplication application;
        private readonly VostokHostingEnvironment environment;
        private readonly ILog log;
        private readonly AtomicBoolean launchedOnce = false;

        public VostokHost([NotNull] VostokHostSettings settings)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));

            application = settings.Application;

            ShutdownTokenSource = new CancellationTokenSource();

            environment = EnvironmentBuilder.Build(settings.EnvironmentSetup, ShutdownTokenSource.Token);

            log = environment.Log.ForContext<VostokHost>();

            onApplicationStateChanged = new CachingObservable<VostokApplicationState>();
            ChangeStateTo(VostokApplicationState.NotInitialized);
        }

        public IObservable<VostokApplicationState> OnApplicationStateChanged => onApplicationStateChanged;

        public async Task<VostokApplicationRunResult> RunAsync()
        {
            LogApplicationIdentity(environment.ApplicationIdentity);

            if (!launchedOnce.TrySetTrue())
                throw new InvalidOperationException("Application can't be launched multiple times.");

            var result = await InitializeApplicationAsync().ConfigureAwait(false)
                         ?? await RunApplicationAsync().ConfigureAwait(false);

            environment.Dispose();

            return result;
        }

        private async Task<VostokApplicationRunResult> InitializeApplicationAsync()
        {
            log.Info("Initializing application.");
            ChangeStateTo(VostokApplicationState.Initializing);

            try
            {
                await application.InitializeAsync(environment);

                log.Info("Application initialization completed successfully.");
                ChangeStateTo(VostokApplicationState.Initialized);

                return null;
            }
            catch (Exception error)
            {
                log.Error(error, "Unhandled exception has occurred while initializing application.");
                ChangeStateTo(VostokApplicationState.CrashedDuringInitialization, error);

                return new VostokApplicationRunResult(VostokApplicationRunStatus.CrashedDuringInitialization, error);
            }
        }

        private async Task<VostokApplicationRunResult> RunApplicationAsync()
        {
            log.Info("Running application.");
            ChangeStateTo(VostokApplicationState.Running);

            try
            {
                var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                var shutdownToken = environment.ShutdownToken;

                using (shutdownToken.Register(o => ((TaskCompletionSource<bool>)o).TrySetCanceled(), tcs))
                {
                    var applicationTask = application.RunAsync(environment);

                    environment.ServiceBeacon.Start();

                    await Task.WhenAny(applicationTask, tcs.Task).ConfigureAwait(false);

                    environment.ServiceBeacon.Stop();

                    if (shutdownToken.IsCancellationRequested)
                    {
                        log.Info("Cancellation requested, waiting for application to complete with timeout = {Timeout}.", settings.ShutdownTimeout);
                        ChangeStateTo(VostokApplicationState.Stopping);

                        if (!await applicationTask.WaitAsync(settings.ShutdownTimeout).ConfigureAwait(false))
                        {
                            throw new OperationCanceledException($"Cancellation requested, but application has not exited within {settings.ShutdownTimeout} timeout.");
                        }

                        log.Info("Application successfully stopped.");
                        ChangeStateTo(VostokApplicationState.Stopped);
                        return new VostokApplicationRunResult(VostokApplicationRunStatus.Stopped);
                    }

                    log.Info("Application exited.");
                    ChangeStateTo(VostokApplicationState.Exited);
                    return new VostokApplicationRunResult(VostokApplicationRunStatus.Exited);
                }
            }
            catch (Exception error)
            {
                log.Error(error, "Unhandled exception has occurred while running application.");
                ChangeStateTo(VostokApplicationState.CrashedDuringRunning, error);
                return new VostokApplicationRunResult(VostokApplicationRunStatus.CrashedDuringRunning, error);
            }
        }

        private void ChangeStateTo(VostokApplicationState newState, Exception error = null)
        {
            ApplicationState = newState;
            onApplicationStateChanged.Next(newState);
            if (error != null)
                onApplicationStateChanged.Error(error);
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
    }
}