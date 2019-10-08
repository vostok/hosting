using JetBrains.Annotations;
using Vostok.Clusterclient.Tracing;
using Vostok.ClusterConfig.Client;
using Vostok.ClusterConfig.Client.Abstractions;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.Components.Configuration
{
    internal class ClusterConfigClientBuilder : IVostokClusterConfigClientBuilder, IBuilder<IClusterConfigClient>
    {
        [NotNull]
        public IClusterConfigClient Build(BuildContext context)
        {
            return new ClusterConfigClient(new ClusterConfigClientSettings
            {
                Log = context.Log,
                AdditionalSetup = setup => setup.SetupDistributedTracing(context.Tracer)
            });
        }
    }
}