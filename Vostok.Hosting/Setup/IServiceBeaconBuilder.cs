using JetBrains.Annotations;
using Vostok.ServiceDiscovery;
using Vostok.ServiceDiscovery.Helpers;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IServiceBeaconBuilder
    {
        IServiceBeaconBuilder SetupReplicaInfo([NotNull] ReplicaInfoSetup replicaInfoSetup);

        IServiceBeaconBuilder SetZooKeeperPathEscaper([NotNull] IZooKeeperPathEscaper pathEscaper);
    }
}