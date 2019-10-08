using JetBrains.Annotations;
using Vostok.ServiceDiscovery;
using Vostok.ServiceDiscovery.Helpers;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokServiceBeaconBuilder
    {
        IVostokServiceBeaconBuilder SetupReplicaInfo([NotNull] ReplicaInfoSetup replicaInfoSetup);

        IVostokServiceBeaconBuilder SetZooKeeperPathEscaper([NotNull] IZooKeeperPathEscaper pathEscaper);
    }
}