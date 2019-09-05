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
    public class VostokHosting : IVostokHosting
    {
        private readonly VostokHostingSettings settings;
        private readonly CachingObservable<VostokApplicationState> onApplicationStateChanged;
        private IVostokApplication application;
        private VostokHostingEnvironment environment;

        public VostokHosting([NotNull] VostokHostingSettings settings)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));

            application = settings.Application;
            environment = EnvironmentBuilder.Build(settings.EnvironmentSetup);

            Log = environment.Log.ForContext<VostokHosting>();

            onApplicationStateChanged = new CachingObservable<VostokApplicationState>(VostokApplicationState.NotInitialized);
        }

        public ILog Log { get; private set; }

        public IObservable<VostokApplicationState> OnApplicationStateChanged => onApplicationStateChanged;

        public async Task RunAsync()
        {
            LogApplicationIdentity(environment.ApplicationIdentity);

            if (await InitializeApplicationAsync().ConfigureAwait(false))
                await RunApplicationAsync().ConfigureAwait(false);

            environment.Dispose();
        }

        private async Task<bool> InitializeApplicationAsync()
        {
            Log.Info("Initializing application.");
            onApplicationStateChanged.Next(VostokApplicationState.Initializing);

            try
            {
                await application.InitializeAsync(environment);

                Log.Info("Initializing application completed successfully.");
                onApplicationStateChanged.Next(VostokApplicationState.Initialized);

                return true;
            }
            catch (Exception error)
            {
                Log.Error(error, "Unhandled exception has occurred while initializing application.");
                onApplicationStateChanged.Error(error);

                return false;
            }
        }

        private async Task RunApplicationAsync()
        {
            Log.Info("Running application.");
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
                        Log.Info("Cancellation requested, waiting for application to complete with {Timeout} timeout.", settings.ShutdownTimeout);
                        onApplicationStateChanged.Next(VostokApplicationState.Stopping);

                        if (!await applicationTask.WaitAsync(settings.ShutdownTimeout).ConfigureAwait(false))
                        {
                            throw new OperationCanceledException($"Cancellation requested, but application has not exited within {settings.ShutdownTimeout} timeout.");
                        }

                        Log.Info("Application successfully stopped.");
                        onApplicationStateChanged.Next(VostokApplicationState.Stopped);
                    }
                    else
                    {
                        Log.Info("Application exited.");
                        onApplicationStateChanged.Next(VostokApplicationState.Exited);
                    }
                }
            }
            catch (Exception error)
            {
                Log.Error(error, "Unhandled exception has occurred while running application.");
                onApplicationStateChanged.Error(error);
            }
        }

        private void LogApplicationIdentity(IVostokApplicationIdentity applicationIdentity)
        {
            Log.Info(
                "Application identity: project: {Project}, environment: {Enrironment}, application: {Application}, instance: {Instance}.",
                applicationIdentity.Project,
                applicationIdentity.Environment,
                applicationIdentity.Application,
                applicationIdentity.Instance);
        }
    }
}