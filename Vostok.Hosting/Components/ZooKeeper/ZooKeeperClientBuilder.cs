using System;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Hosting.Components.ClusterProvider;
using Vostok.Hosting.Helpers;
using Vostok.Hosting.Setup;
using Vostok.ZooKeeper.Client;
using Vostok.ZooKeeper.Client.Abstractions;
// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.ZooKeeper
{
    internal class ZooKeeperClientBuilder : IVostokZooKeeperClientBuilder, IBuilder<IZooKeeperClient>
    {
        private ClusterProviderBuilder clusterProviderBuilder;
        private string connectionString;
        private readonly Customization<ZooKeeperClientSettings> settingsCustomization;

        public ZooKeeperClientBuilder()
        {
            settingsCustomization = new Customization<ZooKeeperClientSettings>();
        }

        public IVostokZooKeeperClientBuilder SetClusterProvider(IClusterProvider clusterProvider)
        {
            connectionString = null;
            clusterProviderBuilder = ClusterProviderBuilder.FromValue(clusterProvider);
            return this;
        }

        public IVostokZooKeeperClientBuilder SetClusterConfigClusterProvider(string path)
        {
            connectionString = null;
            clusterProviderBuilder = ClusterProviderBuilder.FromClusterConfig(path);
            return this;
        }

        public IVostokZooKeeperClientBuilder SetConnectionString(string connectionString)
        {
            clusterProviderBuilder = null;
            this.connectionString = connectionString;
            return this;
        }

        public IVostokZooKeeperClientBuilder CustomizeSettings(Action<ZooKeeperClientSettings> settingsCustomization)
        {
            this.settingsCustomization.AddCustomization(settingsCustomization);
            return this;
        }

        public IZooKeeperClient Build(BuildContext context)
        {
            ZooKeeperClientSettings settings = null;

            if (connectionString != null)
                settings = new ZooKeeperClientSettings(connectionString);
            
            var cluster = clusterProviderBuilder?.Build(context);
            if (cluster != null)
                settings = new ZooKeeperClientSettings(() => cluster.GetCluster());

            if (settings == null)
                return null;

            settingsCustomization.Customize(settings);

            return new ZooKeeperClient(settings, context.Log);
        }
    }
}