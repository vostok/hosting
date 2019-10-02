﻿using Vostok.Clusterclient.Core.Topology;
using Vostok.Hosting.Components.ClusterProvider;
using Vostok.Hosting.Setup;
using Vostok.ZooKeeper.Client;
using Vostok.ZooKeeper.Client.Abstractions;
// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.ZooKeeper
{
    internal class ZooKeeperClientBuilder : IZooKeeperClientBuilder, IBuilder<IZooKeeperClient>
    {
        private ClusterProviderBuilder clusterProviderBuilder;
        private string connectionString;

        public IZooKeeperClientBuilder SetClusterProvider(IClusterProvider clusterProvider)
        {
            connectionString = null;
            clusterProviderBuilder = ClusterProviderBuilder.FromValue(clusterProvider);
            return this;
        }

        public IZooKeeperClientBuilder SetClusterConfigClusterProvider(string path)
        {
            connectionString = null;
            clusterProviderBuilder = ClusterProviderBuilder.FromClusterConfig(path);
            return this;
        }

        public IZooKeeperClientBuilder SetConnectionString(string connectionString)
        {
            clusterProviderBuilder = null;
            this.connectionString = connectionString;
            return this;
        }

        public IZooKeeperClient Build(BuildContext context)
        {
            var cluster = clusterProviderBuilder?.Build(context);
            if (cluster == null)
                return null;

            var settings = new ZooKeeperClientSettings(() => cluster.GetCluster());

            return new ZooKeeperClient(settings, context.Log);
        }
    }
}