using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vostok.Hosting.Models;
using Vostok.Hosting.Snippet;

namespace Vostok.Hosting.VostokMultiHost
{
    public class VostokMultiHost
    {
        public VostokMultiHost(VostokMultiHostSettings settings, params VostokApplicationSettings[] apps)
        {
            throw new NotImplementedException();
        }

        // Initialize environment, run ALL added applications, stop on all apps stopped. You can't run twice.
        public Task<Dictionary<string, VostokApplicationRunResult>> RunAsync() => throw new NotImplementedException();

        // Returns after environment initialization and configuration. You can't start twice (even after being stopped).
        public Task StartAsync() => throw new NotImplementedException();

        // Stop all applications and dispose yourself.
        public Task<Dictionary<string, VostokApplicationRunResult>> StopAsync() => throw new NotImplementedException();
        
        // Waits until all launched apps exit.
        public Task<Dictionary<string, VostokApplicationRunResult>> WaitForExitAsync() => throw new NotImplementedException();

        // Get app or null.
        public VostokMultiHostApplication GetApp(string appName) => throw new NotImplementedException();

        // Add application and return it back. Throws KeyAlreadyExists exception.
        public VostokMultiHostApplication AddApp(VostokApplicationSettings vostokApplicationSettings) => throw new NotImplementedException();
        
        // Get added applications. 
        public IEnumerable<(string appName, VostokMultiHostApplication)> Applications => throw new NotImplementedException();
    }
}