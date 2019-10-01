using JetBrains.Annotations;
using Vostok.ServiceDiscovery.Helpers;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IServiceLocatorBuilder
    {
        IServiceLocatorBuilder SetZooKeeperPathEscaper([NotNull] IZooKeeperPathEscaper pathEscaper);
    }
}