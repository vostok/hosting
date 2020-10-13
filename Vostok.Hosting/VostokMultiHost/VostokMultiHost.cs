using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private CommonBuildContext CommonContext { get; } = new CommonBuildContext();

        public VostokMultiHost(VostokMultiHostSettings settings, params VostokApplicationSettings[] apps)
        {
            this.settings = settings;
            applications = new ConcurrentDictionary<string, IVostokMultiHostApplication>();

            foreach (var app in apps)
                AddApp(app);
        }

        // Get added applications. 
        public IEnumerable<(string appName, IVostokMultiHostApplication)> Applications => applications.Select(x => (x.Key, x.Value)).ToArray();

        // Initialize environment, run ALL added applications, stop on all apps stopped. You can't run twice.
        public Task<Dictionary<string, VostokApplicationRunResult>> RunAsync()
        {
            // TODO: Check state and set state afterwards
            
            BuildCommonContext();
            
            throw new NotImplementedException();
        }

        // Returns after environment initialization and configuration. You can't start twice (even after being stopped).
        public Task StartAsync()
        {
            // TODO: Check state and set state afterwards    
            BuildCommonContext();
            
            return Task.CompletedTask;
        }

        // Stop all applications and dispose yourself.
        public Task<Dictionary<string, VostokApplicationRunResult>> StopAsync() => throw new NotImplementedException();

        // Get app or null.
        public IVostokMultiHostApplication GetApp(string appName) => applications.ContainsKey(appName) ? applications[appName] : null;
        
        public IVostokMultiHostApplication AddApp(VostokApplicationSettings vostokApplicationSettings)
        {
            if (applications.ContainsKey(vostokApplicationSettings.ApplicationName))
                throw new ArgumentException("Application with this name has already been added.");
            
            return applications[vostokApplicationSettings.ApplicationName] = new VostokMultiHostApplication(vostokApplicationSettings, CommonContext);
        }

        public void RemoveApp(string appName) => applications.TryRemove(appName, out var _);

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
                EnvironmentBuilder.BuildCommonContext(CommonContext, settings.EnvironmentSetup, environmentFactorySettings);
            }
            catch (Exception error)
            {
                // TODO: Log event and set state. 
                throw;
            }
        }

        protected VostokMultiHostSettings settings { get; set; }
    }
}