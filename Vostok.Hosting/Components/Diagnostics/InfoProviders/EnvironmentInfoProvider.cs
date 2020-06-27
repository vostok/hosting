using System;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using Vostok.Commons.Environment;
using Vostok.Datacenters;
using Vostok.Hosting.Abstractions.Diagnostics;

namespace Vostok.Hosting.Components.Diagnostics.InfoProviders
{
    internal class EnvironmentInfoProvider : IDiagnosticInfoProvider
    {
        private readonly IDatacenters datacenters;

        public EnvironmentInfoProvider(IDatacenters datacenters)
            => this.datacenters = datacenters;

        public object Query() => new
        {
            EnvironmentInfo.Host,
            EnvironmentInfo.FQDN,
            Datacenter = datacenters.GetLocalDatacenter(),
            EnvironmentInfo.ProcessId,
            EnvironmentInfo.ProcessName,
            EnvironmentInfo.BaseDirectory,
            System.Environment.CurrentDirectory,
            System.Environment.UserName,
            RuntimeInformation.OSDescription,
            RuntimeInformation.FrameworkDescription,
            Bitness = System.Environment.Is64BitProcess ? "x64" : "x86",
            Uptime = DateTime.Now - Process.GetCurrentProcess().StartTime,
            GCSettings.IsServerGC
        };
    }
}
