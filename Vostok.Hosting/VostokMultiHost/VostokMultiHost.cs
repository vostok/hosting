using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vostok.Hosting.Models;

namespace Vostok.Hosting.VostokMultiHost
{
    public class VostokMultiHost
    {
        private readonly ConcurrentDictionary<string, IVostokMultiHostApplication> applications;

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
        public Task<Dictionary<string, VostokApplicationRunResult>> RunAsync() => throw new NotImplementedException();

        // Returns after environment initialization and configuration. You can't start twice (even after being stopped).
        public Task StartAsync() => throw new NotImplementedException();

        // Stop all applications and dispose yourself.
        public Task<Dictionary<string, VostokApplicationRunResult>> StopAsync() => throw new NotImplementedException();

        // Get app or null.
        public IVostokMultiHostApplication GetApp(string appName) => applications.ContainsKey(appName) ? applications[appName] : null;

        public IVostokMultiHostApplication AddApp(VostokApplicationSettings vostokApplicationSettings)
        {
            if (applications.ContainsKey(vostokApplicationSettings.ApplicationName))
                throw new ArgumentException("Application with this name has already been added.");
            
            // application[vostokApplicationSettings.ApplicationName];
            throw new NotImplementedException();
        }

        public void RemoveApp(string appName) => applications.TryRemove(appName, out var _);

        protected VostokMultiHostSettings settings { get; set; }
    }
}