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
            InstanceIndex = instanceIndex;
            InstancesCount = instancesCount;
        }

        /// <inheritdoc />
        public int InstanceIndex { get; }

        /// <inheritdoc />
        public int InstancesCount { get; }
    }
}