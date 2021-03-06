﻿using System;
using Vostok.Hosting.Setup;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Application
{
    internal class ApplicationLimitsBuilder : IVostokApplicationLimitsBuilder, IBuilder<ApplicationLimits>
    {
        private volatile Func<float?> cpuUnitsProvider;
        private volatile Func<long?> memoryBytesProvider;

        public ApplicationLimitsBuilder()
        {
            cpuUnitsProvider = () => null;
            memoryBytesProvider = () => null;
        }

        public IVostokApplicationLimitsBuilder SetCpuUnits(float? cpuUnits)
        {
            cpuUnitsProvider = () => cpuUnits;
            return this;
        }

        public IVostokApplicationLimitsBuilder SetCpuUnitsProvider(Func<float?> cpuUnitsProvider)
        {
            this.cpuUnitsProvider = cpuUnitsProvider ?? throw new ArgumentNullException(nameof(cpuUnitsProvider));
            return this;
        }

        public IVostokApplicationLimitsBuilder SetMemoryBytes(long? memoryBytes)
        {
            memoryBytesProvider = () => memoryBytes;
            return this;
        }

        public IVostokApplicationLimitsBuilder SetMemoryBytesProvider(Func<long?> memoryBytesProvider)
        {
            this.memoryBytesProvider = memoryBytesProvider ?? throw new ArgumentNullException(nameof(memoryBytesProvider));
            return this;
        }

        public ApplicationLimits Build(BuildContext context) =>
            new ApplicationLimits(cpuUnitsProvider, memoryBytesProvider);
    }
}