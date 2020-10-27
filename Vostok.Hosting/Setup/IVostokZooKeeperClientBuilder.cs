using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Topology;
using Vostok.ZooKeeper.Client;
using Vostok.ZooKeeper.Client.Abstractions;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokZooKeeperClientBuilder
    {
        bool IsEnabled { get; }

        IVostokZooKeeperClientBuilder Disable();
        IVostokZooKeeperClientBuilder UseInstance(IZooKeeperClient instance);

        IVostokZooKeeperClientBuilder SetClusterProvider([NotNull] IClusterProvider clusterProvider);
        IVostokZooKeeperClientBuilder SetClusterConfigTopology([NotNull] string path);
        IVostokZooKeeperClientBuilder SetConnectionString([NotNull] string connectionString);
        IVostokZooKeeperClientBuilder SetAuthentication(string login, string apiKey);
        IVostokZooKeeperClientBuilder CustomizeSettings([NotNull] Action<ZooKeeperClientSettings> settingsCustomization);
    }
}