using System;
using JetBrains.Annotations;
using Vostok.Hosting.Abstractions;

namespace Vostok.Hosting.Components.Application
{
    internal class ApplicationLimits : IVostokApplicationLimits
    {
        private readonly Func<float?> cpuUnitsProvider;
        private readonly Func<long?> memoryBytesProvider;

        public ApplicationLimits([NotNull] Func<float?> cpuUnitsProvider, [NotNull] Func<long?> memoryBytesProvider)
        {
            this.cpuUnitsProvider = cpuUnitsProvider ?? throw new ArgumentNullException(nameof(cpuUnitsProvider));
            this.memoryBytesProvider = memoryBytesProvider ?? throw new ArgumentNullException(nameof(memoryBytesProvider));
        }

        public float? CpuUnits => cpuUnitsProvider();
        public long? MemoryBytes => memoryBytesProvider();
    }
}