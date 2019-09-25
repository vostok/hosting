using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Topology.SD;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.Components.ClusterProvider
{
    internal class ServiceDiscoveryClusterProviderBuilder : IBuilder<IClusterProvider>
    {
        private readonly string environment;
        private readonly string application;

        public ServiceDiscoveryClusterProviderBuilder(string environment, string application)
        {
            this.environment = environment;
            this.application = application;
        }

        public IClusterProvider Build(BuildContext context)
        {
            return context.ServiceLocator == null
                ? null
                : new ServiceDiscoveryClusterProvider(context.ServiceLocator, environment, application, context.Log);
        }
    }
}