using System;
using System.Diagnostics;
using Vostok.Commons.Threading;
using Vostok.Configuration.Primitives;
using Vostok.Hosting.Abstractions.Diagnostics;

namespace Vostok.Hosting.Components.Diagnostics.InfoProviders
{
    internal class SystemMetricsProvider : IDiagnosticInfoProvider
    {
        public object Query()
        {
            var currentProcess = Process.GetCurrentProcess();
            var workingSet = currentProcess.WorkingSet64;
            var virtualMemory = currentProcess.VirtualMemorySize64;
            var managedMemory = GC.GetTotalMemory(false);

            return new
            {
                ThreadPool = ThreadPoolUtility.GetPoolState(),
                Memory = new
                {
                    WorkingSet = workingSet,
                    WorkingSetPretty = workingSet.Bytes(),
                    VirtualMemory = virtualMemory,
                    VirtualMemoryPretty = virtualMemory.Bytes(),
                    ManagedMemory = managedMemory,
                    ManagedMemoryPretty = managedMemory.Bytes()
                }
            };
        }
    }
}
