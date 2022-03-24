using Vostok.ClusterConfig.Client.Abstractions;
using Vostok.Configuration.Abstractions;
using Vostok.Datacenters;
using Vostok.Hosting.Models;
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
            IConfigurationSource mergedConfigurationSource,
            IConfigurationProvider configurationProvider,
            IConfigurationProvider secretConfigurationProvider,
            IClusterConfigClient clusterConfigClient,
            IDatacenters datacenters)
        {
            Log = log.ForContext<VostokHostingEnvironment>();
            
            ConfigurationSource = configurationSource;
            SecretConfigurationSource = secretConfigurationSource;
            MergedConfigurationSource = mergedConfigurationSource;
            
            ConfigurationProvider = configurationProvider;
            SecretConfigurationProvider = secretConfigurationProvider;
            
            ClusterConfigClient = clusterConfigClient;
            
            Datacenters = datacenters;
        }

        public ILog Log { get; }
        public IConfigurationSource ConfigurationSource { get; }
        public IConfigurationSource SecretConfigurationSource { get; }
        public IConfigurationSource MergedConfigurationSource { get; }
        
        public IConfigurationProvider ConfigurationProvider { get; }
        public IConfigurationProvider SecretConfigurationProvider { get; }
        
        public IClusterConfigClient ClusterConfigClient { get; }
        
        public IDatacenters Datacenters { get; }
    }
}