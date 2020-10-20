using System;
using System.Threading.Tasks;
using Vostok.Configuration.Abstractions.Extensions.Observable;
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
            parentMultiHost.AddRunningApp(Name, this);
            return Task.WhenAny(vostokHost.RunAsync(), RemoveRunningApp()).Result as Task<VostokApplicationRunResult>;
        }

        public Task StartAsync(VostokApplicationState? stateToAwait)
        {
            CreateVostokHost();
            parentMultiHost.AddRunningApp(Name, this);
            return Task.WhenAny(vostokHost.StartAsync(), RemoveRunningApp());
        }

        public Task<VostokApplicationRunResult> StopAsync(bool ensureSuccess = true)
        {
            return vostokHost?.StopAsync(ensureSuccess) ?? Task.FromResult(new VostokApplicationRunResult(VostokApplicationState.NotInitialized));
        }

        private VostokApplicationSettings settings { get; }
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

        private async Task RemoveRunningApp()
        {
            var stateCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            var subscription = vostokHost.OnApplicationStateChanged.Subscribe(
                state =>
                {
                    if (state.IsTerminal())
                    {
                        parentMultiHost.RemoveRunningApp(Name);
                        stateCompletionSource.TrySetResult(true);
                    }
                });

            using (subscription)
                await stateCompletionSource.Task;
        }
    }
}