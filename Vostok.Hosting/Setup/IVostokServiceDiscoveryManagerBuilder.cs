using System;
using JetBrains.Annotations;
using Vostok.ServiceDiscovery;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokServiceDiscoveryManagerBuilder
    {
        IVostokServiceDiscoveryManagerBuilder CustomizeSettings([NotNull] Action<ServiceDiscoveryManagerSettings> settingsCustomization);
    }
}