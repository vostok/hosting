using Vostok.Clusterclient.Core.Topology;

namespace Vostok.Hosting.Components.ClusterProvider
{
    internal class CustomClusterProviderBuilder : IBuilder<IClusterProvider>
    {
        private readonly IClusterProvider clusterProvider;

        public CustomClusterProviderBuilder(IClusterProvider clusterProvider)
        {
            this.clusterProvider = clusterProvider;
        }

        public IClusterProvider Build(Context context) => clusterProvider;
    }
}