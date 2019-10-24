using Vostok.Hosting.Setup;
// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Application
{
    internal class ApplicationLimitsBuilder : IVostokApplicationLimitsBuilder, IBuilder<ApplicationLimits>
    {
        private float? cpuUnits;
        private long? memoryBytes;

        public IVostokApplicationLimitsBuilder SetCpuUnits(float? cpuUnits)
        {
            this.cpuUnits = cpuUnits;
            return this;
        }

        public IVostokApplicationLimitsBuilder SetMemoryBytes(long? memoryBytes)
        {
            this.memoryBytes = memoryBytes;
            return this;
        }

        public ApplicationLimits Build(BuildContext context) =>
            new ApplicationLimits(cpuUnits, memoryBytes);
    }
}