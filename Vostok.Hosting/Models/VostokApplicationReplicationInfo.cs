using JetBrains.Annotations;
using Vostok.Hosting.Abstractions;

namespace Vostok.Hosting.Models
{
    /// <inheritdoc />
    [PublicAPI]
    public class VostokApplicationReplicationInfo : IVostokApplicationReplicationInfo
    {
        /// <inheritdoc />
        public int InstanceIndex { get; }

        /// <inheritdoc />
        public int InstancesCount { get; }

        public VostokApplicationReplicationInfo(int instanceIndex, int instancesCount)
        {
            InstanceIndex = instanceIndex;
            InstancesCount = instancesCount;
        }
    }
}