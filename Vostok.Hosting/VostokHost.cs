using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Commons.Helpers.Extensions;
using Vostok.Commons.Helpers.Observable;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Components.Environment;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting
{
    [PublicAPI]
    public class VostokHost
    {
        public VostokApplicationState ApplicationState;
        private readonly VostokHostSettings settings;
        private readonly CachingObservable<VostokApplicationState> onApplicationStateChanged;
        private readonly IVostokApplication application;
        private readonly VostokHostingEnvironment environment;
        private readonly ILog log;

        public VostokHost([NotNull] VostokHostSettings settings)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));

            application = settings.Application;
            environment = EnvironmentBuilder.Build(settings.EnvironmentSetup);

            log = environment.Log.ForContext<VostokHost>();

            onApplicationStateChanged = new CachingObservable<VostokApplicationState>();
            ChangeStateTo(VostokApplicationState.NotInitialized);
        }

        public IObservable<VostokApplicationState> OnApplicationStateChanged => onApplicationStateChanged;

        public async Task<RunResult> RunAsync()
        {
            LogApplicationIdentity(environment.ApplicationIdentity);

            var result = await InitializeApplicationAsync().ConfigureAwait(false)
                         ?? await RunApplicationAsync().ConfigureAwait(false);

            environment.Dispose();

            return result;
        }

        private async Task<RunResult> InitializeApplicationAsync()
        {
            log.Info("Initializing application.");
            ChangeStateTo(VostokApplicationState.Initializing);

            try
            {
                await application.InitializeAsync(environment);

                log.Info("Initializing application completed successfully.");
                ChangeStateTo(VostokApplicationState.Initialized);

                return null;
            }
            catch (Exception error)
            {
                log.Error(error, "Unhandled exception has occurred while initializing application.");
                ChangeStateTo(VostokApplicationState.Crashed, error);

                return new RunResult(RunResultStatus.ApplicationCrashed, error);
            }
        }

        private async Task<RunResult> RunApplicationAsync()
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
                        log.Info("Cancellation requested, waiting for application to complete with {Timeout} timeout.", settings.ShutdownTimeout);
                        ChangeStateTo(VostokApplicationState.Stopping);

                        if (!await applicationTask.WaitAsync(settings.ShutdownTimeout).ConfigureAwait(false))
                        {
                            throw new OperationCanceledException($"Cancellation requested, but application has not exited within {settings.ShutdownTimeout} timeout.");
                        }

                        log.Info("Application successfully stopped.");
                        ChangeStateTo(VostokApplicationState.Stopped);
                        return new RunResult(RunResultStatus.ApplicationStopped);
                    }

                    log.Info("Application exited.");
                    ChangeStateTo(VostokApplicationState.Exited);
                    return new RunResult(RunResultStatus.ApplicationExited);
                }
            }
            catch (Exception error)
            {
                log.Error(error, "Unhandled exception has occurred while running application.");
                ChangeStateTo(VostokApplicationState.Crashed, error);
                return new RunResult(RunResultStatus.ApplicationCrashed, error);
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
            log.Info(
                "Application identity: project: {Project}, environment: {Enrironment}, application: {Application}, instance: {Instance}.",
                applicationIdentity.Project,
                applicationIdentity.Environment,
                applicationIdentity.Application,
                applicationIdentity.Instance);
        }
    }
}