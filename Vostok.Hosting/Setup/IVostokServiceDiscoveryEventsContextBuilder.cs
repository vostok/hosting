using System;
using JetBrains.Annotations;
using Vostok.ServiceDiscovery.Telemetry;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokServiceDiscoveryEventsContextBuilder
    {
        IVostokServiceDiscoveryEventsContextBuilder SetupHerculesServiceDiscoveryEventsSender([NotNull] Action<IVostokHerculesServiceDiscoveryEventsSenderBuilder> herculesEventsSenderSetup);

        IVostokServiceDiscoveryEventsContextBuilder CustomizeSettings([NotNull] Action<ServiceDiscoveryEventsContextConfig> settingsCustomization);
    }
}