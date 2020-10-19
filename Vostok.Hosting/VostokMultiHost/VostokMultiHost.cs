using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vostok.Commons.Helpers.Observable;
using Vostok.Commons.Threading;
using Vostok.Hosting.Components;
using Vostok.Hosting.Components.Environment;
using Vostok.Hosting.Models;

namespace Vostok.Hosting.VostokMultiHost
{
    public class VostokMultiHost
    {
        // TODO: Add state. 
        private readonly ConcurrentDictionary<string, IVostokMultiHostApplication> applications;
        private readonly AtomicBoolean launchedOnce = false;

        public VostokMultiHost(VostokMultiHostSettings settings, params VostokApplicationSettings[] apps)
        {
            this.settings = settings;
            applications = new ConcurrentDictionary<string, IVostokMultiHostApplication>();

            foreach (var app in apps)
                AddApp(app);
            
            onHostStateChanged = new CachingObservable<VostokMultiHostState>();
        }

        public VostokMultiHostState HostState { get; private set; }

        private readonly CachingObservable<VostokMultiHostState> onHostStateChanged;
        
        // Get added applications. 
        public IEnumerable<(string appName, IVostokMultiHostApplication)> Applications => applications.Select(x => (x.Key, x.Value));

        // TODO: Wrap Dictionary in some kind of VostokMultiHostRunResult. (In order to handle crashes and errors.)
        // Initialize environment, run ALL added applications, stop when apps have stopped. You can't run twice.
        public Task<VostokMultiHostRunResult> RunAsync()
        {
            // TODO: Check state and set state afterwards
            if (!launchedOnce.TrySetTrue())
                throw new InvalidOperationException("VostokMultiHost can't be launched multiple times.");

            return Task.Run(RunInternalAsync);
        }

        // Returns after environment initialization and configuration. You can't start twice (even after being stopped).
        public async Task StartAsync()
        {
            // TODO: Add state and subscription. Then make this similar to VostokHost.
            var initializationTask = Task.Run(
                async () =>
                {
                    while (CommonContext == null)
                        await Task.Delay(50);    
                });

            var runnerTask = RunAsync().ContinueWith(task => task.Result.EnsureSuccess(), TaskContinuationOptions.OnlyOnRanToCompletion);

            var completedTask = Task.WhenAny(runnerTask, initializationTask);

            await completedTask.ConfigureAwait(false);
        }

        // Stop all applications and dispose yourself.
        // TODO: Make parallel.
        public async Task<VostokMultiHostRunResult> StopAsync()
        {
            // TODO: Add lock and inspect Tasks.
            var resultDict = new Dictionary<string, VostokApplicationRunResult>();
            foreach (var (appName, vostokMultiHostApplication) in Applications)
                resultDict[appName] = await vostokMultiHostApplication.StopAsync();
            CommonContext.DisposeCommonComponents();
            return resultDict;
        }

        // Get app or null.
        public IVostokMultiHostApplication GetApp(string appName) => applications.TryGetValue(appName, out var app) ? app : null;

        public IVostokMultiHostApplication AddApp(VostokApplicationSettings vostokApplicationSettings)
        {
            if (applications.ContainsKey(vostokApplicationSettings.ApplicationName))
                throw new ArgumentException("Application with this name has already been added.");

            return applications[vostokApplicationSettings.ApplicationName] = new VostokMultiHostApplication(vostokApplicationSettings, this);
        }

        public void RemoveApp(string appName) => applications.TryRemove(appName, out var _);

        protected VostokMultiHostSettings settings { get; set; }
        internal CommonBuildContext CommonContext { get; set; }

        private async Task<Dictionary<string, VostokApplicationRunResult>> RunInternalAsync()
        {
            BuildCommonContext();
            await Task.WhenAll(applications.Select(x => x.Value.RunAsync()));
            return await StopAsync();
        }

        // TODO: Return state as VostokHost does?
        private void BuildCommonContext()
        {
            var environmentFactorySettings = new VostokHostingEnvironmentFactorySettings
            {
                ConfigureStaticProviders = settings.ConfigureStaticProviders,
                BeaconShutdownTimeout = settings.BeaconShutdownTimeout,
                BeaconShutdownWaitEnabled = settings.BeaconShutdownWaitEnabled
            };

            try
            {
                CommonContext = EnvironmentBuilder.BuildCommonContext(settings.EnvironmentSetup, environmentFactorySettings);
            }
            catch (Exception error)
            {
                // TODO: Log event and set state. 
                throw;
            }
        }
        
        private VostokMultiHostRunResult ReturnResult(VostokMultiHostState newState, Exception error = null)
        {
            ChangeStateTo(newState, error);
            return new VostokMultiHostRunResult(newState, error);
        }

        private void ChangeStateTo(VostokMultiHostState newState, Exception error = null)
        {
            HostState = newState;

            onHostStateChanged.Next(newState);

            if (error != null)
                onHostStateChanged.Error(error);
            else if (newState.IsTerminal())
                onHostStateChanged.Complete();
        }
    }
}