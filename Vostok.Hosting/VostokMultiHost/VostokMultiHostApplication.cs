using System;
using System.Threading.Tasks;
using Vostok.Hosting.Models;
using Vostok.Hosting.Snippet;

namespace Vostok.Hosting.VostokMultiHost
{
    public class VostokMultiHostApplication
    {
        private VostokHost vostokHost;

        public VostokMultiHostApplication(VostokApplicationSettings settings)
        {
            vostokHostSettings = settings;
        }

        public string Name { get; }

        public VostokApplicationState ApplicationState => throw new NotImplementedException();

        public Task<VostokApplicationRunResult> RunAsync() => throw new NotImplementedException();

        public Task StartAsync(VostokApplicationState? stateToAwait = VostokApplicationState.Running) => throw new NotImplementedException();

        // Dispose and delete VostokHost in order to recreate new one later (if needed).
        public Task<VostokApplicationRunResult> StopAsync(bool ensureSuccess = true) => throw new NotImplementedException();

        protected VostokApplicationSettings vostokHostSettings { get; }
    }
}