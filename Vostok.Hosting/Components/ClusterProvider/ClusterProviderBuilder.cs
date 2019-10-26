using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Topology.CC;
using Vostok.Clusterclient.Topology.SD;

namespace Vostok.Hosting.Components.ClusterProvider
{
    internal class ClusterProviderBuilder : IBuilder<IClusterProvider>
    {
        private readonly Func<BuildContext, IClusterProvider> provider;

        private ClusterProviderBuilder(Func<BuildContext, IClusterProvider> provider)
            => this.provider = provider;

        public static ClusterProviderBuilder FromValue(IClusterProvider value)
            => new ClusterProviderBuilder(context => value);

        public static ClusterProviderBuilder FromClusterConfig(string path) =>
            new ClusterProviderBuilder(
                context =>
                    context.ClusterConfigClient == null
                        ? null
                        : new ClusterConfigClusterProvider(context.ClusterConfigClient, path, context.Log));

        public static ClusterProviderBuilder FromServiceDiscovery(string environment, string application) =>
            new ClusterProviderBuilder(
                context =>
                    context.ServiceLocator == null
                        ? null
                        : new ServiceDiscoveryClusterProvider(context.ServiceLocator, environment, application, context.Log));

        [CanBeNull]
        public IClusterProvider Build(BuildContext context) => provider(context);
    }
}