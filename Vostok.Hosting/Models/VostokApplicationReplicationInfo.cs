using JetBrains.Annotations;
using Vostok.Hosting.Abstractions;

namespace Vostok.Hosting.Models
{
    [PublicAPI]
    public class VostokApplicationReplicationInfo : IVostokApplicationReplicationInfo
    {
        public int InstanceIndex { get; }
        public int InstancesCount { get; }

        public VostokApplicationReplicationInfo(int instanceIndex, int instancesCount)
        {
            InstanceIndex = instanceIndex;
            InstancesCount = instancesCount;
        }
    }
}