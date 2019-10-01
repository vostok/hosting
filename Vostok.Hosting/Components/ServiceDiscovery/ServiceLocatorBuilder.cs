using Vostok.Hosting.Setup;
using Vostok.ServiceDiscovery;
using Vostok.ServiceDiscovery.Abstractions;
using Vostok.ServiceDiscovery.Helpers;
// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.ServiceDiscovery
{
    internal class ServiceLocatorBuilder : IServiceLocatorBuilder, IBuilder<IServiceLocator>
    {
        private IZooKeeperPathEscaper pathEscaper;

        public IServiceLocator Build(BuildContext context)
        {
            var zooKeeperClient = context.ZooKeeperClient;

            if (zooKeeperClient == null)
                return new DevNullServiceLocator();

            return new ServiceLocator(zooKeeperClient, new ServiceLocatorSettings());
        }

        public IServiceLocatorBuilder SetZooKeeperPathEscaper(IZooKeeperPathEscaper pathEscaper)
        {
            this.pathEscaper = pathEscaper;
            return this;
        }
    }
}