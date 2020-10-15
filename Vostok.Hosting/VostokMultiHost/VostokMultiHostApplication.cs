using System;
using System.Threading.Tasks;
using Vostok.Hosting.Components;
using Vostok.Hosting.Models;

namespace Vostok.Hosting.VostokMultiHost
{
    // TODO: Inspect this class one more time
    internal class VostokMultiHostApplication : IVostokMultiHostApplication
    {
        public VostokMultiHostApplication(VostokApplicationSettings settings, VostokMultiHost parentMultiHost)
        {
            this.settings = settings;
            this.parentMultiHost = parentMultiHost;
        }

        public string Name => settings.ApplicationName;
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

        public Task<VostokApplicationRunResult> StopAsync(bool ensureSuccess = true) => vostokHost.StopAsync(ensureSuccess);

        protected VostokApplicationSettings settings { get; }
        private VostokHost vostokHost { get; set; }
        private VostokMultiHost parentMultiHost { get; }

        private void CreateVostokHost()
        {
            // TODO: Propagate settings from VostokApplicationSettings
            var vostokHostSettings = new VostokHostSettings(settings.Application, settings.EnvironmentSetup)
            {
                ConfigureThreadPool = false,
                CommonBuildContext = (parentMultiHost.CommonContext ?? throw new InvalidOperationException("VostokMultiHost should be started to run applications.")).Clone()
            };

            vostokHost = new VostokHost(vostokHostSettings);
        }
    }
}