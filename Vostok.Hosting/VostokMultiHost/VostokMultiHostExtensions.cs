using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vostok.Hosting.Models;

namespace Vostok.Hosting.VostokMultiHost
{
    public static class VostokMultiHostExtensions
    {
        public static Task<VostokApplicationRunResult> RunApp(this VostokMultiHost host, VostokApplicationSettings settings)
        {
            return host.AddApp(settings).RunAsync();
        }

        public static Task StartApp(this VostokMultiHost host, VostokApplicationSettings settings)
        {
            return host.AddApp(settings).StartAsync();
        }

        public static Task<VostokApplicationRunResult> StopApp(this VostokMultiHost host, string appName)
        {
            return host.GetApp(appName).StopAsync();
        }

        public static Task StartSequentially(this VostokMultiHost host, IEnumerable<VostokApplicationSettings> apps)
        {
            return Task.Run(
                () =>
                {
                    foreach (var app in apps)
                        host.StartApp(app).GetAwaiter().GetResult();
                });
        }

        public static Task StartSequentially(this VostokMultiHost host, params VostokApplicationSettings[] apps)
        {
            return StartSequentially(host, (IEnumerable<VostokApplicationSettings>)apps);
        }

        public static Task StartInParallel(this VostokMultiHost host, IEnumerable<VostokApplicationSettings> apps)
        {
            return Task.WhenAll(apps.Select(app => StartApp(host, app)));
        }

        public static Task StartInParallel(this VostokMultiHost host, params VostokApplicationSettings[] apps)
        {
            return StartInParallel(host, (IEnumerable<VostokApplicationSettings>)apps);
        }
    }
}