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

        public VostokMultiHostApplication(VostokMultiHostApplicationSettings settings, Func<bool> isReadyToStart)
        {
            Settings = settings;
            this.isReadyToStart = isReadyToStart;
        }

        public string Name => Settings.ApplicationName;
        public VostokApplicationState ApplicationState => vostokHost?.ApplicationState ?? VostokApplicationState.NotInitialized;

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
        private VostokMultiHostApplicationSettings Settings { get; }

        private void CreateVostokHost()
        {
            if (!isReadyToStart())
                throw new InvalidOperationException("VostokMultiHost should be running to launch applications!");

            if (!launchedOnce.TrySetTrue())
                throw new InvalidOperationException("IVostokMultiHostApplication can't be launched more than once!");

            var vostokHostSettings = new VostokHostSettings(Settings.Application, Settings.EnvironmentSetup)
            {
                ConfigureThreadPool = false,
                ConfigureStaticProviders = false
            };

            vostokHost = new VostokHost(vostokHostSettings);
        }
    }
}