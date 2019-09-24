using JetBrains.Annotations;
using Vostok.ClusterConfig.Client;
using Vostok.ClusterConfig.Client.Abstractions;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.Components.Configuration
{
    internal class ClusterConfigClientBuilder : IClusterConfigClientBuilder, IBuilder<IClusterConfigClient>
    {
        [NotNull]
        public IClusterConfigClient Build(Context context)
        {
            return new ClusterConfigClient(new ClusterConfigClientSettings
            {
                Log = context.Log
            });
        }
    }
}