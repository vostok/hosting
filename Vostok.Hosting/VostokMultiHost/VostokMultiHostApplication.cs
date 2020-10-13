using System;
using System.Threading.Tasks;
using Vostok.Hosting.Components;
using Vostok.Hosting.Models;

namespace Vostok.Hosting.VostokMultiHost
{
    internal class VostokMultiHostApplication : IVostokMultiHostApplication
    {
        public VostokMultiHostApplication(VostokApplicationSettings settings, BuildContext commonContext)
        {
            this.settings = settings;
            this.commonContext = commonContext;
        }

        public string Name { get; }
        public VostokApplicationState ApplicationState { get; }

        public Task<VostokApplicationRunResult> RunAsync()
        {
            var vostokHostSettings = new VostokHostSettings(settings.Application, settings.EnvironmentSetup);
            
            // vostokHost = new VostokHost(vostokHostSettings, commonContext);
            throw new NotImplementedException();
        }

        public Task StartAsync(VostokApplicationState? stateToAwait) =>
            throw new NotImplementedException();

        public Task<VostokApplicationRunResult> StopAsync(bool ensureSuccess = true) =>
            throw new NotImplementedException();

        protected VostokApplicationSettings settings { get; set; }
        protected BuildContext commonContext { get; set; }
        private VostokHost vostokHost { get; set; }
    }
}