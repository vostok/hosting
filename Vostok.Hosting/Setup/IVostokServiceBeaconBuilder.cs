using System;
using JetBrains.Annotations;
using Vostok.ServiceDiscovery;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokServiceBeaconBuilder
    {
        IVostokServiceBeaconBuilder Disable();

        IVostokServiceBeaconBuilder SetupReplicaInfo([NotNull] ReplicaInfoSetup replicaInfoSetup);

        IVostokServiceBeaconBuilder CustomizeSettings([NotNull] Action<ServiceBeaconSettings> settingsCustomization);
    }
}