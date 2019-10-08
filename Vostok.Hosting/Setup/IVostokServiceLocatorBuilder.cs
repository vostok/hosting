using JetBrains.Annotations;
using Vostok.ServiceDiscovery.Helpers;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokServiceLocatorBuilder
    {
        IVostokServiceLocatorBuilder SetZooKeeperPathEscaper([NotNull] IZooKeeperPathEscaper pathEscaper);
    }
}