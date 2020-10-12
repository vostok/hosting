using System;
using System.Threading.Tasks;
using Vostok.Hosting.Models;

namespace Vostok.Hosting.VostokMultiHost
{
    internal class VostokMultiHostApplication : IVostokMultiHostApplication
    {
        public VostokMultiHostApplication(VostokApplicationSettings settings)
        {
            this.settings = settings;
        }

        public string Name { get; }
        public VostokApplicationState ApplicationState { get; }

        public Task<VostokApplicationRunResult> RunAsync() =>
            throw new NotImplementedException();

        public Task StartAsync(VostokApplicationState? stateToAwait) =>
            throw new NotImplementedException();

        public Task<VostokApplicationRunResult> StopAsync(bool ensureSuccess = true) =>
            throw new NotImplementedException();

        protected VostokApplicationSettings settings { get; set; }
        private VostokHost vostokHost { get; set; }
    }
}