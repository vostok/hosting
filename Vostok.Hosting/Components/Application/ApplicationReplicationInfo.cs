using System;
using Vostok.Hosting.Abstractions;

namespace Vostok.Hosting.Components.Application
{
    internal class ApplicationReplicationInfo : IVostokApplicationReplicationInfo
    {
        public ApplicationReplicationInfo(int instanceIndex, int instancesCount)
        {
            if (!(0 <= instanceIndex && instanceIndex < instancesCount))
                throw new ArgumentOutOfRangeException($"Instance index not in range [{instanceIndex}, {instancesCount}).");

            InstanceIndex = instanceIndex;
            InstancesCount = instancesCount;
        }

        public int InstanceIndex { get; }
        public int InstancesCount { get; }
    }
}