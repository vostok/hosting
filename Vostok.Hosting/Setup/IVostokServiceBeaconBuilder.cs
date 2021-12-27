using System;
using JetBrains.Annotations;
using Vostok.ServiceDiscovery;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokServiceBeaconBuilder
    {
        bool IsEnabled { get; }
        IVostokServiceBeaconBuilder Enable();
        IVostokServiceBeaconBuilder Disable();

        IVostokServiceBeaconBuilder DenyRegistrationFromNotActiveDatacenters();

        IVostokServiceBeaconBuilder AllowRegistrationFromNotActiveDatacenters();
        
        IVostokServiceBeaconBuilder SetupHerculesServiceDiscoveryEventsSender([NotNull] Action<IVostokHerculesServiceDiscoveryEventsSenderBuilder> herculesEventsSenderSetup);

        IVostokServiceBeaconBuilder SetupReplicaInfo([NotNull] ReplicaInfoSetup replicaInfoSetup);

        IVostokServiceBeaconBuilder CustomizeSettings([NotNull] Action<ServiceBeaconSettings> settingsCustomization);
    }
}