﻿using System;
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
using Vostok.ZooKeeper.Client.Abstractions.Model.Authentication;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.ZooKeeper
{
    internal class ZooKeeperClientBuilder : SwitchableComponent<IVostokZooKeeperClientBuilder>,
        IVostokZooKeeperClientBuilder,
        IBuilder<IZooKeeperClient>
    {
        public readonly Customization<IZooKeeperClient> StaticProviderCustomization;
        private readonly Customization<ZooKeeperClientSettings> settingsCustomization;
        private readonly List<AuthenticationInfo> authenticationInfos;
        private volatile ClusterProviderBuilder clusterProviderBuilder;
        private volatile string connectionString;
        private volatile IZooKeeperClient instance;

        public ZooKeeperClientBuilder()
        {
            StaticProviderCustomization = new Customization<IZooKeeperClient>();
            settingsCustomization = new Customization<ZooKeeperClientSettings>();
            authenticationInfos = new List<AuthenticationInfo>();
        }

        public IVostokZooKeeperClientBuilder UseInstance(IZooKeeperClient instance)
        {
            this.instance = instance;

            return this;
        }

        public IVostokZooKeeperClientBuilder SetClusterProvider(IClusterProvider clusterProvider)
        {
            instance = null;
            connectionString = null;

            clusterProviderBuilder = ClusterProviderBuilder.FromValue(clusterProvider ?? throw new ArgumentNullException(nameof(clusterProvider)));

            return this;
        }

        public IVostokZooKeeperClientBuilder SetClusterConfigTopology(string path)
        {
            instance = null;
            connectionString = null;

            clusterProviderBuilder = ClusterProviderBuilder.FromClusterConfig(path ?? throw new ArgumentNullException(nameof(path)));

            return this;
        }

        public IVostokZooKeeperClientBuilder SetConnectionString(string connectionString)
        {
            instance = null;
            clusterProviderBuilder = null;

            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

            return this;
        }

        public IVostokZooKeeperClientBuilder AddAuthenticationInfo(AuthenticationInfo authenticationInfo)
        {
            authenticationInfos.Add(authenticationInfo);

            return this;
        }

        public IVostokZooKeeperClientBuilder CustomizeSettings(Action<ZooKeeperClientSettings> settingsCustomization)
        {
            instance = null;

            this.settingsCustomization.AddCustomization(settingsCustomization ?? throw new ArgumentNullException(nameof(settingsCustomization)));

            return this;
        }

        public IVostokZooKeeperClientBuilder ConfigureStaticProvider(Action<IZooKeeperClient> configure)
        {
            StaticProviderCustomization.AddCustomization(configure ?? throw new ArgumentNullException(nameof(configure)));

            return this;
        }

        public IZooKeeperClient Build(BuildContext context)
        {
            if (!IsEnabled)
            {
                context.LogDisabled("ZooKeeperClient");
                return new DevNullZooKeeperClient();
            }

            if (instance != null)
            {
                context.ExternalComponents.Add(instance);

                return instance;
            }

            ZooKeeperClientSettings settings = null;

            if (connectionString != null)
                settings = new ZooKeeperClientSettings(connectionString);

            var cluster = clusterProviderBuilder?.Build(context);
            if (cluster != null)
                settings = new ZooKeeperClientSettings(() => cluster.GetCluster());

            if (settings == null)
            {
                context.LogDisabled("ZooKeeperClient", "unconfigured ZooKeeper topology");
                return new DevNullZooKeeperClient();
            }

            settingsCustomization.Customize(settings);

            var zkClient = new ZooKeeperClient(
                settings,
                context.Log.WithEventsDroppedByProperties(IsDataChangedLog));

            foreach (var authenticationInfo in authenticationInfos)
                zkClient.AddAuthenticationInfo(authenticationInfo);

            return zkClient;
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