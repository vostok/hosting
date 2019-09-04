using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Commons.Helpers.Extensions;
using Vostok.Commons.Helpers.Observable;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting
{
    [PublicAPI]
    public class VostokHosting
    {
        private readonly VostokHostingSettings settings;
        private IVostokApplication application;
        private VostokHostingEnvironment environment;
        private readonly CachingObservable<ApplicationState> onApplicationStateChanged;
        private ILog log;

        public VostokHosting([NotNull] VostokHostingSettings settings)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));

            application = settings.Application;
            onApplicationStateChanged = new CachingObservable<ApplicationState>(ApplicationState.NotInitialized);
        }

        public async Task RunAsync()
        {
            // TODO(kungurtsev): build environment.
            environment = settings.Environment;
            log = environment.Log.ForContext<VostokHosting>();

            LogApplicationIdentity(environment.ApplicationIdentity);

            if (await InitializeApplicationAsync().ConfigureAwait(false))
                await RunApplicationAsync().ConfigureAwait(false);

            environment.Dispose();
        }

        public IObservable<ApplicationState> OnApplicationStateChanged => onApplicationStateChanged;

        private async Task<bool> InitializeApplicationAsync()
        {
            log.Info("Initializing application.");
            onApplicationStateChanged.Next(ApplicationState.Initializing);

            try
            {
                await application.InitializeAsync(environment);

                log.Info("Initializing application completed successfully.");
                onApplicationStateChanged.Next(ApplicationState.Initialized);

                return true;
            }
            catch (Exception error)
            {
                log.Error(error, "Unhandled exception has occurred while initializing application.");
                onApplicationStateChanged.Error(error);

                return false;
            }
        }

        private async Task RunApplicationAsync()
        {
            log.Info("Running application.");
            onApplicationStateChanged.Next(ApplicationState.Running);

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
                        onApplicationStateChanged.Next(ApplicationState.Stopping);

                        if (!await applicationTask.WaitAsync(settings.ShutdownTimeout).ConfigureAwait(false))
                        {
                            throw new OperationCanceledException($"Cancellation requested, but application has not exited within {settings.ShutdownTimeout} timeout.");
                        }

                        log.Info("Application successfully stopped.");
                        onApplicationStateChanged.Next(ApplicationState.Stopped);
                    }
                    else
                    {
                        log.Info("Application exited.");
                        onApplicationStateChanged.Next(ApplicationState.Exited);
                    }
                }
            }
            catch (Exception error)
            {
                log.Error(error, "Unhandled exception has occurred while running application.");
                onApplicationStateChanged.Error(error);
            }
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