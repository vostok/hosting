using System;
using JetBrains.Annotations;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokApplicationLimitsBuilder
    {
        IVostokApplicationLimitsBuilder SetCpuUnits(float? cpuUnits);
        IVostokApplicationLimitsBuilder SetCpuUnitsProvider(Func<float?> cpuUnitsProvider);

        IVostokApplicationLimitsBuilder SetMemoryBytes(long? memoryBytes);
        IVostokApplicationLimitsBuilder SetMemoryBytesProvider(Func<long?> memoryBytesProvider);
    }
}