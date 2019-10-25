using System;
using JetBrains.Annotations;
using Vostok.Hosting.Abstractions;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokApplicationReplicationInfoBuilder
    {
        IVostokApplicationReplicationInfoBuilder SetReplicationInfo([NotNull] IVostokApplicationReplicationInfo replicationInfo);

        IVostokApplicationReplicationInfoBuilder SetReplicationInfoProvider([NotNull] Func<IVostokApplicationReplicationInfo> replicationInfoProvider);
    }
}