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
            IConfigurationSource secretConfigurationSource,
            IConfigurationProvider configurationProvider,
            IConfigurationProvider secretConfigurationProvider,
            IClusterConfigClient clusterConfigClient)
        {
            ConfigurationSource = configurationSource;
            ConfigurationProvider = configurationProvider;
            SecretConfigurationSource = secretConfigurationSource;
            SecretConfigurationProvider = secretConfigurationProvider;
            ClusterConfigClient = clusterConfigClient;
        }

        public IConfigurationSource ConfigurationSource { get; }
        public IConfigurationSource SecretConfigurationSource { get; }
        public IConfigurationProvider ConfigurationProvider { get; }
        public IConfigurationProvider SecretConfigurationProvider { get; }
        public IClusterConfigClient ClusterConfigClient { get; }
    }
}