using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Topology;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IZooKeeperClientBuilder
    {
        IZooKeeperClientBuilder SetClusterProvider([NotNull] IClusterProvider clusterProvider);
        IZooKeeperClientBuilder SetClusterConfigClusterProvider([NotNull] string path);
        IZooKeeperClientBuilder SetConnectionString([NotNull] string connectionString);
    }
}