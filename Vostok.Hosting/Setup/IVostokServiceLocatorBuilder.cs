using System;
using JetBrains.Annotations;
using Vostok.ServiceDiscovery;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokServiceLocatorBuilder
    {
        IVostokServiceLocatorBuilder CustomizeSettings([NotNull] Action<ServiceLocatorSettings> settingsCustomization);
    }
}