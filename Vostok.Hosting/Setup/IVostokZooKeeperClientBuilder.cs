using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Topology;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokZooKeeperClientBuilder
    {
        IVostokZooKeeperClientBuilder SetClusterProvider([NotNull] IClusterProvider clusterProvider);
        IVostokZooKeeperClientBuilder SetClusterConfigClusterProvider([NotNull] string path);
        IVostokZooKeeperClientBuilder SetConnectionString([NotNull] string connectionString);

        // CR(iloktionov): Apply this approach to every implementation with its own non-trivial configuration.
        // CR(iloktionov): What if we'll have to use values from configuration?
        // IZooKeeperClientBuilder Customize([NotNull] Action<ZooKeeperClientSettings> customization);
    }
}