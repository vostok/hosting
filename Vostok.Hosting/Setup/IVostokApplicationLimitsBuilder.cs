using System;
using JetBrains.Annotations;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokApplicationLimitsBuilder
    {
        IVostokApplicationLimitsBuilder SetCpuUnits(float? cpuUnits);
        IVostokApplicationLimitsBuilder SetCpuUnitsProvider([NotNull] Func<float?> cpuUnitsProvider);

        IVostokApplicationLimitsBuilder SetMemoryBytes(long? memoryBytes);
        IVostokApplicationLimitsBuilder SetMemoryBytesProvider([NotNull] Func<long?> memoryBytesProvider);
    }
}