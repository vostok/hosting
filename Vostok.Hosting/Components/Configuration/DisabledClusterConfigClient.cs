using Vostok.ClusterConfig.Client;

namespace Vostok.Hosting.Components.Configuration
{
    internal class DisabledClusterConfigClient : ClusterConfigClient
    {
        public DisabledClusterConfigClient()
            : base(
                new ClusterConfigClientSettings
                {
                    EnableLocalSettings = false,
                    EnableClusterSettings = false
                })
        {
        }
    }
}
