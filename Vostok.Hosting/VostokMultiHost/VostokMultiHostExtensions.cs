using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vostok.Hosting.Models;

namespace Vostok.Hosting.VostokMultiHost
{
    public static class VostokMultiHostExtensions
    {
        public static Task<VostokApplicationRunResult> RunApp(this Hosting.VostokMultiHost.VostokMultiHost host, VostokApplicationSettings settings)
        {
            // Can be ran only after VostokMultiHost start.
            return host.AddApp(settings).RunAsync();
        }
        
        public static Task StartApp(this Hosting.VostokMultiHost.VostokMultiHost host, VostokApplicationSettings settings)
        {
            // Can be ran only after VostokMultiHost start.
            throw new NotImplementedException();
        }
        
        public static Task RestartApp(this Hosting.VostokMultiHost.VostokMultiHost host, string appName)
        {
            // Can be ran only after VostokMultiHost start.
            throw new NotImplementedException();
        }
        
        public static Task<VostokApplicationRunResult> StopApp(this Hosting.VostokMultiHost.VostokMultiHost host, string appName)
        {
            // Can be ran only after VostokMultiHost start.
            throw new NotImplementedException();
        }
        
        public static Task StartSequentially(this Hosting.VostokMultiHost.VostokMultiHost host, IEnumerable<VostokApplicationSettings> apps)
        {
            // Can be ran only after VostokMultiHost start.
            throw new NotImplementedException();
        }
        
        public static Task StartSequentially(this Hosting.VostokMultiHost.VostokMultiHost host, params VostokApplicationSettings[] apps)
        {
            // Can be ran only after VostokMultiHost start.
            throw new NotImplementedException();
        }
        
        public static Task StartInParallel(this Hosting.VostokMultiHost.VostokMultiHost host, IEnumerable<VostokApplicationSettings> apps)
        {
            // Can be ran only after VostokMultiHost start.
            throw new NotImplementedException();
        }
        
        public static Task StartInParallel(this Hosting.VostokMultiHost.VostokMultiHost host, params VostokApplicationSettings[] apps)
        {
            // Can be ran only after VostokMultiHost start.
            throw new NotImplementedException();
        }
        
        // ...
    }
}