using System;
using System.Threading.Tasks;
using Vostok.Commons.Threading;
using Vostok.Hosting.Models;

namespace Vostok.Hosting.MultiHost
{
    internal class VostokMultiHostApplication : IVostokMultiHostApplication
    {
        private readonly AtomicBoolean launchedOnce = false;
        private readonly Func<bool> isReadyToStart;
        private volatile VostokHost vostokHost;
        private readonly VostokMultiHostApplicationSettings settings;

        public VostokMultiHostApplication(VostokMultiHostApplicationSettings settings, Func<bool> isReadyToStart)
        {
            this.settings = settings;
            this.isReadyToStart = isReadyToStart;
        }

        public string Name => settings.ApplicationName;

        public VostokApplicationState ApplicationState => vostokHost?.ApplicationState ?? VostokApplicationState.NotInitialized;

        // TODO: Make run idempotent?
        // CR(iloktionov): RunAsync should be idempotent!
        public Task<VostokApplicationRunResult> RunAsync()
        {
            CreateVostokHost();

            return vostokHost.RunAsync();
        }

        public Task StartAsync(VostokApplicationState? stateToAwait)
        {
            CreateVostokHost();

            return vostokHost.StartAsync(stateToAwait);
        }

        public Task<VostokApplicationRunResult> StopAsync(bool ensureSuccess = true)
        {
            return vostokHost?.StopAsync(ensureSuccess) ?? Task.FromResult(new VostokApplicationRunResult(VostokApplicationState.NotInitialized));
        }

        internal Task<VostokApplicationRunResult> WorkerTask => vostokHost?.workerTask;

        // CR(iloktionov): Reformat with code style? And maybe convert to a private readonly field?

        private void CreateVostokHost()
        {
            if (!isReadyToStart())
                throw new InvalidOperationException("VostokMultiHost should be running to launch applications!");

            if (!launchedOnce.TrySetTrue())
                throw new InvalidOperationException("IVostokMultiHostApplication can't be launched more than once!");

            var vostokHostSettings = new VostokHostSettings(settings.Application, settings.EnvironmentSetup)
            {
                ConfigureThreadPool = false,
                ConfigureStaticProviders = false
            };

            vostokHost = new VostokHost(vostokHostSettings);
        }
    }
}