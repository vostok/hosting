using System;
using System.Threading.Tasks;
using Vostok.Commons.Threading;
using Vostok.Configuration.Abstractions.Extensions.Observable;
using Vostok.Hosting.Models;
// ReSharper disable InconsistentlySynchronizedField

namespace Vostok.Hosting.MultiHost
{
    internal class VostokMultiHostApplication : IVostokMultiHostApplication
    {
        private readonly AtomicBoolean launchedOnce = false;
        private readonly object launchGate = new object();
        private readonly Func<bool> isReadyToStart;
        private readonly VostokMultiHostApplicationSettings settings;
        private volatile VostokHost vostokHost;

        public VostokMultiHostApplication(VostokMultiHostApplicationSettings settings, Func<bool> isReadyToStart)
        {
            this.settings = settings;
            this.isReadyToStart = isReadyToStart;
        }

        public string Name => settings.ApplicationName;

        public VostokApplicationState ApplicationState => vostokHost?.ApplicationState ?? VostokApplicationState.NotInitialized;

        public Task<VostokApplicationRunResult> RunAsync()
        {
            lock (launchGate)
                CreateVostokHost();

            return WorkerTask;
        }

        public async Task StartAsync(VostokApplicationState? stateToAwait)
        {
            lock(launchGate)
                CreateVostokHost();
            
            var stateCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            var subscription = vostokHost.OnApplicationStateChanged.Subscribe(
                state =>
                {
                    if (state == stateToAwait)
                        stateCompletionSource.TrySetResult(true);
                });

            using (subscription)
            {
                var runnerTask = WorkerTask.ContinueWith(task => task.Result.EnsureSuccess(), TaskContinuationOptions.OnlyOnRanToCompletion);

                if (stateToAwait == null)
                    return;

                var completedTask = await Task.WhenAny(runnerTask, stateCompletionSource.Task).ConfigureAwait(false);

                await completedTask.ConfigureAwait(false);
            }
        }

        public Task<VostokApplicationRunResult> StopAsync(bool ensureSuccess = true)
        {
            return vostokHost?.StopAsync(ensureSuccess) ?? Task.FromResult(new VostokApplicationRunResult(VostokApplicationState.NotInitialized));
        }

        internal volatile Task<VostokApplicationRunResult> WorkerTask;

        private void CreateVostokHost()
        {
            if (!launchedOnce.TrySetTrue())
                return;
            
            if (!isReadyToStart())
                throw new InvalidOperationException("VostokMultiHost should be running to launch applications.");

            var vostokHostSettings = new VostokHostSettings(settings.Application, settings.EnvironmentSetup)
            {
                ConfigureThreadPool = false,
                ConfigureStaticProviders = false
            };

            vostokHost = new VostokHost(vostokHostSettings);

            WorkerTask = Task.Run(vostokHost.RunAsync);
        }
    }
}