using Vostok.Hosting.Abstractions;

namespace Vostok.Hosting.Components.Application
{
    internal class ApplicationLimits : IVostokApplicationLimits
    {
        public ApplicationLimits(float? cpuUnits, long? memoryBytes)
        {
            CpuUnits = cpuUnits;
            MemoryBytes = memoryBytes;
        }

        public float? CpuUnits { get; }
        public long? MemoryBytes { get; }
    }
}