using System;
using System.Threading.Tasks;
using Vostok.Hosting.Components;
using Vostok.Hosting.Models;

namespace Vostok.Hosting.VostokMultiHost
{
    // TODO: Inspect this class one more time
    internal class VostokMultiHostApplication : IVostokMultiHostApplication
    {
        public VostokMultiHostApplication(VostokApplicationSettings settings, CommonBuildContext commonContext)
        {
            this.settings = settings;
            this.commonContext = commonContext;
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
        protected CommonBuildContext commonContext { get; }
        private VostokHost vostokHost { get; set; }

        private void CreateVostokHost()
        {
            // TODO: Propagate settings from VostokApplicationSettings
            var vostokHostSettings = new VostokHostSettings(settings.Application, settings.EnvironmentSetup)
            {
                ConfigureThreadPool = false,
                CommonBuildContext = commonContext.Clone()
            };

            vostokHost = new VostokHost(vostokHostSettings);
        }
    }
}