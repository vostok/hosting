using System;
using System.Threading.Tasks;
using Vostok.Commons.Threading;
using Vostok.Hosting.Models;

namespace Vostok.Hosting.MultiHost
{
    internal class VostokMultiHostApplication : IVostokMultiHostApplication
    {
        private readonly AtomicBoolean launchedOnce = false;
        private volatile VostokHost vostokHost;

        public VostokMultiHostApplication(VostokMultiHostApplicationSettings settings)
        {
            Settings = settings;
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
            if (!launchedOnce.TrySetTrue())
                throw new InvalidOperationException("VostokHost can't be launched more than once!");

            var vostokHostSettings = new VostokHostSettings(Settings.Application, Settings.EnvironmentSetup)
            {
                ConfigureThreadPool = false,
                ConfigureStaticProviders = false
            };

            vostokHost = new VostokHost(vostokHostSettings);
        }
    }
}