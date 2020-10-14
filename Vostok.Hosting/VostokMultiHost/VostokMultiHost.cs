using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vostok.Commons.Threading;
using Vostok.Hosting.Components;
using Vostok.Hosting.Components.Environment;
using Vostok.Hosting.Models;

namespace Vostok.Hosting.VostokMultiHost
{
    // TODO: Dispose after stop.
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
        }

        // Get added applications. 
        public IEnumerable<(string appName, IVostokMultiHostApplication)> Applications => applications.Select(x => (x.Key, x.Value)).ToArray();

        // TODO: Wrap Dictionary in some kind of VostokMultiHostRunResult. (In order to handle crashes and errors.)
        // Initialize environment, run ALL added applications, stop on all apps stopped. You can't run twice.
        public Task<Dictionary<string, VostokApplicationRunResult>> RunAsync()
        {
            // TODO: Check state and set state afterwards
            if (!launchedOnce.TrySetTrue())
                throw new InvalidOperationException("VostokMultiHost can't be launched multiple times.");

            return Task.Run(RunInternalAsync);
        }

        // Returns after environment initialization and configuration. You can't start twice (even after being stopped).
        public Task StartAsync()
        {
            BuildCommonContext();

            return Task.CompletedTask;
        }

        // Stop all applications and dispose yourself.
        // TODO: Make parallel.
        public async Task<Dictionary<string, VostokApplicationRunResult>> StopAsync()
        {
            // TODO: Add lock and inspect Tasks.
            var resultDict = new Dictionary<string, VostokApplicationRunResult>();
            foreach (var (appName, vostokMultiHostApplication) in Applications)
                resultDict[appName] = await vostokMultiHostApplication.StopAsync();
            CommonContext.DisposeCommonComponents();
            return resultDict;
        }

        // Get app or null.
        public IVostokMultiHostApplication GetApp(string appName) => applications.ContainsKey(appName) ? applications[appName] : null;

        public IVostokMultiHostApplication AddApp(VostokApplicationSettings vostokApplicationSettings)
        {
            if (applications.ContainsKey(vostokApplicationSettings.ApplicationName))
                throw new ArgumentException("Application with this name has already been added.");

            return applications[vostokApplicationSettings.ApplicationName] = new VostokMultiHostApplication(vostokApplicationSettings, this);
        }

        public void RemoveApp(string appName) => applications.TryRemove(appName, out var _);

        protected VostokMultiHostSettings settings { get; set; }
        // TODO: Handle case when StartAsync not called. (It could either be try/catch or just another way or propagating common build context).
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
    }
}