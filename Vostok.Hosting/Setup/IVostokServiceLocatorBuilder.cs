using System;
using JetBrains.Annotations;
using Vostok.ServiceDiscovery;
using Vostok.ServiceDiscovery.Abstractions;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokServiceLocatorBuilder
    {
        IVostokServiceLocatorBuilder CustomizeSettings([NotNull] Action<ServiceLocatorSettings> settingsCustomization);

        IVostokServiceLocatorBuilder ConfigureStaticProvider([NotNull] Action<IServiceLocator> configure);
    }
}