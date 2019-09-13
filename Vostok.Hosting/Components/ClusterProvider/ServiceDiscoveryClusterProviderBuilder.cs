using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Topology.SD;

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

        public IClusterProvider Build(Context context)
        {
            return context.ServiceLocator == null
                ? null
                : new ServiceDiscoveryClusterProvider(context.ServiceLocator, environment, application, context.Log);
        }
    }
}