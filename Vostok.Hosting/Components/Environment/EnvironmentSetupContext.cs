using Vostok.ClusterConfig.Client.Abstractions;
using Vostok.Configuration.Abstractions;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.Components.Environment
{
    internal class EnvironmentSetupContext : IVostokHostingEnvironmentSetupContext
    {
        public EnvironmentSetupContext(
            ILog log,
            IConfigurationSource configurationSource,
            IConfigurationSource secretConfigurationSource,
            IConfigurationProvider configurationProvider,
            IConfigurationProvider secretConfigurationProvider,
            IClusterConfigClient clusterConfigClient)
        {
            Log = log;
            ConfigurationSource = configurationSource;
            ConfigurationProvider = configurationProvider;
            SecretConfigurationSource = secretConfigurationSource;
            SecretConfigurationProvider = secretConfigurationProvider;
            ClusterConfigClient = clusterConfigClient;
        }

        public ILog Log { get; }
        public IConfigurationSource ConfigurationSource { get; }
        public IConfigurationSource SecretConfigurationSource { get; }
        public IConfigurationProvider ConfigurationProvider { get; }
        public IConfigurationProvider SecretConfigurationProvider { get; }
        public IClusterConfigClient ClusterConfigClient { get; }
    }
}