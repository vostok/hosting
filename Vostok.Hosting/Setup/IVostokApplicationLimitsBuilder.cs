using JetBrains.Annotations;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokApplicationLimitsBuilder
    {
        IVostokApplicationLimitsBuilder SetCpuUnits(float? cpuUnits);

        IVostokApplicationLimitsBuilder SetMemoryBytes(long? memoryBytes);
    }
}