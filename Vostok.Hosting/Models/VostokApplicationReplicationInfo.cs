using System;
using JetBrains.Annotations;
using Vostok.Hosting.Abstractions;

namespace Vostok.Hosting.Models
{
    /// <inheritdoc />
    [PublicAPI]
    public class VostokApplicationReplicationInfo : IVostokApplicationReplicationInfo
    {
        public VostokApplicationReplicationInfo(int instanceIndex, int instancesCount)
        {
            if (!(0 <= instanceIndex && instanceIndex < instancesCount))
                throw new ArgumentOutOfRangeException($"Instance index ({instanceIndex}) not in range [0, {instancesCount - 1}].");

            InstanceIndex = instanceIndex;
            InstancesCount = instancesCount;
        }

        /// <inheritdoc />
        public int InstanceIndex { get; }

        /// <inheritdoc />
        public int InstancesCount { get; }
    }
}