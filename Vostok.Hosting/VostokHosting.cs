using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Commons.Helpers.Extensions;
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
        private ILog log;

        public VostokHosting([NotNull] VostokHostingSettings settings)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));

            application = settings.Application;
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

        private async Task<bool> InitializeApplicationAsync()
        {
            log.Info("Initializing application.");

            try
            {
                await application.InitializeAsync(environment);
                log.Info("Initializing application completed successfully.");
                return true;
            }
            catch (Exception error)
            {
                log.Error(error, "Unhandled exception has occurred while initializing application.");
                settings.OnError?.Invoke(error);
                return false;
            }
        }

        private async Task RunApplicationAsync()
        {
            log.Info("Running application.");

            try
            {
                var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                var shutdownToken = environment.ShutdownToken;

                using (shutdownToken.Register(environment.ServiceBeacon.Stop))
                using (shutdownToken.Register(o => ((TaskCompletionSource<bool>)o).TrySetCanceled(), tcs))
                {
                    environment.ServiceBeacon.Start();

                    var applicationTask = application.RunAsync(environment);

                    await Task.WhenAny(applicationTask, tcs.Task).ConfigureAwait(false);

                    if (shutdownToken.IsCancellationRequested)
                    {
                        log.Info("Cancellation requested, waiting for application to complete with {Timeout} timeout.", settings.ShutdownTimeout);
                        if (!await applicationTask.WaitAsync(settings.ShutdownTimeout).ConfigureAwait(false))
                        {
                            throw new OperationCanceledException($"Cancellation requested, but application has not exited within {settings.ShutdownTimeout} timeout.");
                        }

                        log.Info("Application successfully stopped.");
                    }
                    else
                    {
                        log.Info("Application exited.");
                    }
                }
            }
            catch (Exception error)
            {
                log.Error(error, "Unhandled exception has occurred while running application.");
                settings.OnError?.Invoke(error);
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