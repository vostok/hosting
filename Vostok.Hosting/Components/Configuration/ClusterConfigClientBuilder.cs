using JetBrains.Annotations;
using Vostok.ClusterConfig.Client;
using Vostok.ClusterConfig.Client.Abstractions;

namespace Vostok.Hosting.Components.Configuration
{
    internal class ClusterConfigClientBuilder : IBuilder<IClusterConfigClient>
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