using System;
using JetBrains.Annotations;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokApplicationLimitsBuilder
    {
        IVostokApplicationLimitsBuilder SetCpuUnits([CanBeNull] float? cpuUnits);
        IVostokApplicationLimitsBuilder SetCpuUnitsProvider([NotNull] Func<float?> cpuUnitsProvider);

        IVostokApplicationLimitsBuilder SetMemoryBytes([CanBeNull] long? memoryBytes);
        IVostokApplicationLimitsBuilder SetMemoryBytesProvider([NotNull] Func<long?> memoryBytesProvider);
    }
}