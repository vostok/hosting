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
    public class VostokHosting
    {
        private readonly VostokHostingSettings settings;
        private readonly CachingObservable<VostokApplicationState> onApplicationStateChanged;
        private IVostokApplication application;
        private VostokHostingEnvironment environment;
        private ILog log;

        public VostokHosting([NotNull] VostokHostingSettings settings)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));

            application = settings.Application;
            environment = EnvironmentBuilder.Build(settings.EnvironmentSetup);

            onApplicationStateChanged = new CachingObservable<VostokApplicationState>(VostokApplicationState.NotInitialized);
        }

        public IObservable<VostokApplicationState> OnApplicationStateChanged => onApplicationStateChanged;

        public async Task RunAsync()
        {
            log = environment.Log.ForContext<VostokHosting>();

            LogApplicationIdentity(environment.ApplicationIdentity);

            if (await InitializeApplicationAsync().ConfigureAwait(false))
                await RunApplicationAsync().ConfigureAwait(false);

            environment.Dispose();
        }

        private async Task<bool> InitializeApplicationAsync()
        {
            log.Info("Initializing application.");
            onApplicationStateChanged.Next(VostokApplicationState.Initializing);

            try
            {
                await application.InitializeAsync(environment);

                log.Info("Initializing application completed successfully.");
                onApplicationStateChanged.Next(VostokApplicationState.Initialized);

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
            onApplicationStateChanged.Next(VostokApplicationState.Running);

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
                        onApplicationStateChanged.Next(VostokApplicationState.Stopping);

                        if (!await applicationTask.WaitAsync(settings.ShutdownTimeout).ConfigureAwait(false))
                        {
                            throw new OperationCanceledException($"Cancellation requested, but application has not exited within {settings.ShutdownTimeout} timeout.");
                        }

                        log.Info("Application successfully stopped.");
                        onApplicationStateChanged.Next(VostokApplicationState.Stopped);
                    }
                    else
                    {
                        log.Info("Application exited.");
                        onApplicationStateChanged.Next(VostokApplicationState.Exited);
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