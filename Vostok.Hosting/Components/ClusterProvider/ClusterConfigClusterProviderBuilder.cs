using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Topology.CC;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.Components.ClusterProvider
{
    internal class ClusterConfigClusterProviderBuilder : IBuilder<IClusterProvider>
    {
        private readonly string path;

        public ClusterConfigClusterProviderBuilder(string path)
        {
            this.path = path;
        }

        public IClusterProvider Build(BuildContext context)
        {
            return context.ClusterConfigClient == null 
                ? null 
                : new ClusterConfigClusterProvider(context.ClusterConfigClient, path, context.Log);
        }
    }
}