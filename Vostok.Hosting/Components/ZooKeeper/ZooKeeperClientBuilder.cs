using System;
using System.Collections.Generic;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Commons.Helpers;
using Vostok.Hosting.Components.ClusterProvider;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.ServiceDiscovery;
using Vostok.ZooKeeper.Client;
using Vostok.ZooKeeper.Client.Abstractions;
using Vostok.ZooKeeper.Client.Abstractions.Model;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.ZooKeeper
{
    internal class ZooKeeperClientBuilder : IVostokZooKeeperClientBuilder, IBuilder<IZooKeeperClient>
    {
        private readonly Customization<ZooKeeperClientSettings> settingsCustomization;
        private volatile ClusterProviderBuilder clusterProviderBuilder;
        private volatile string connectionString;

        public ZooKeeperClientBuilder()
        {
            settingsCustomization = new Customization<ZooKeeperClientSettings>();
        }

        public IVostokZooKeeperClientBuilder SetClusterProvider(IClusterProvider clusterProvider)
        {
            connectionString = null;
            clusterProviderBuilder = ClusterProviderBuilder.FromValue(clusterProvider ?? throw new ArgumentNullException(nameof(clusterProvider)));
            return this;
        }

        public IVostokZooKeeperClientBuilder SetClusterConfigTopology(string path)
        {
            connectionString = null;
            clusterProviderBuilder = ClusterProviderBuilder.FromClusterConfig(path ?? throw new ArgumentNullException(nameof(path)));
            return this;
        }

        public IVostokZooKeeperClientBuilder SetConnectionString(string connectionString)
        {
            clusterProviderBuilder = null;
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            return this;
        }

        public IVostokZooKeeperClientBuilder CustomizeSettings(Action<ZooKeeperClientSettings> settingsCustomization)
        {
            this.settingsCustomization.AddCustomization(settingsCustomization ?? throw new ArgumentNullException(nameof(settingsCustomization)));
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
            {
                context.LogDisabled("ZooKeeperClient", "unconfigured ZooKeeper topology");
                return null;
            }

            settingsCustomization.Customize(settings);

            return new ZooKeeperClient(
                settings,
                context.Log.WithEventsDroppedByProperties(IsDataChangedLog));
        }

        private bool IsDataChangedLog(IReadOnlyDictionary<string, object> properties)
        {
            if (!properties.TryGetValue("NodeEventType", out var eventType))
                return false;
            if (!properties.TryGetValue("NodePath", out var path))
                return false;
            return
                eventType is NodeChangedEventType castedEventType && castedEventType == NodeChangedEventType.DataChanged &&
                path is string castedPath && castedPath.StartsWith(new ServiceLocatorSettings().ZooKeeperNodesPrefix);
        }
    }
}