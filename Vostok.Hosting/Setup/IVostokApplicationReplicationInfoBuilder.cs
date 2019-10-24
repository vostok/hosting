using System;
using JetBrains.Annotations;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokApplicationReplicationInfoBuilder
    {
        IVostokApplicationReplicationInfoBuilder SetReplicationInfo(int instanceIndex, int instancesCount);

        IVostokApplicationReplicationInfoBuilder SetReplicationInfoProvider([NotNull] Func<(int instanceIndex, int instancesCount)> replicationInfoProvider);
    }
}