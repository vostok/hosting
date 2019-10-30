using Vostok.ClusterConfig.Client.Abstractions;
using Vostok.Configuration.Abstractions;
using Vostok.Hosting.Setup;

// ReSharper disable NotNullMemberIsNotInitialized

namespace Vostok.Hosting.Components.Configuration
{
    internal class ConfigurationContext : IVostokConfigurationContext
    {
        public ConfigurationContext(
            IConfigurationSource configurationSource,
            IConfigurationProvider configurationProvider,
            IClusterConfigClient clusterConfigClient)
        {
            ConfigurationSource = configurationSource;
            ConfigurationProvider = configurationProvider;
            ClusterConfigClient = clusterConfigClient;
        }

        public IConfigurationSource ConfigurationSource { get; }
        public IConfigurationProvider ConfigurationProvider { get; }
        public IClusterConfigClient ClusterConfigClient { get; }
    }
}