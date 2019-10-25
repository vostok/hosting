using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Topology;
using Vostok.ZooKeeper.Client;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokZooKeeperClientBuilder
    {
        IVostokZooKeeperClientBuilder SetClusterProvider([NotNull] IClusterProvider clusterProvider);
        IVostokZooKeeperClientBuilder SetClusterConfigTopology([NotNull] string path);
        IVostokZooKeeperClientBuilder SetConnectionString([NotNull] string connectionString);

        IVostokZooKeeperClientBuilder CustomizeSettings([NotNull] Action<ZooKeeperClientSettings> settingsCustomization);
    }
}