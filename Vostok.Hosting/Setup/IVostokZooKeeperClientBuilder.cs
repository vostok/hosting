using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Topology;
using Vostok.ZooKeeper.Client;
using Vostok.ZooKeeper.Client.Abstractions;
using Vostok.ZooKeeper.Client.Abstractions.Model.Authentication;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokZooKeeperClientBuilder
    {
        bool IsEnabled { get; }
        IVostokZooKeeperClientBuilder Enable();
        IVostokZooKeeperClientBuilder Disable();

        IVostokZooKeeperClientBuilder UseInstance(IZooKeeperClient instance);

        IVostokZooKeeperClientBuilder SetClusterProvider([NotNull] IClusterProvider clusterProvider);
        IVostokZooKeeperClientBuilder SetClusterConfigTopology([NotNull] string path);
        IVostokZooKeeperClientBuilder SetConnectionString([NotNull] string connectionString);
        IVostokZooKeeperClientBuilder AddAuthenticationInfo([NotNull] AuthenticationInfo authenticationInfo);
        IVostokZooKeeperClientBuilder CustomizeSettings([NotNull] Action<ZooKeeperClientSettings> settingsCustomization);
    }
}