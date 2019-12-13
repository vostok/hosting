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
            IClusterConfigClient clusterConfigClient)
        {
            Log = log;
            ConfigurationSource = configurationSource;
            SecretConfigurationSource = secretConfigurationSource;
            ConfigurationProvider = configurationProvider;
            ClusterConfigClient = clusterConfigClient;
        }

        public ILog Log { get; }
        public IConfigurationSource ConfigurationSource { get; }
        public IConfigurationSource SecretConfigurationSource { get; }
        public IConfigurationProvider ConfigurationProvider { get; }
        public IClusterConfigClient ClusterConfigClient { get; }
    }
}