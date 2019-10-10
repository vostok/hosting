using Vostok.ClusterConfig.Client.Abstractions;
using Vostok.Configuration.Abstractions;
using Vostok.Hosting.Setup;
// ReSharper disable NotNullMemberIsNotInitialized

namespace Vostok.Hosting.Components.Configuration
{
    internal class ConfigurationContext : IVostokConfigurationContext
    {
        public IConfigurationSource ConfigurationSource { get; set; }
        public IConfigurationProvider ConfigurationProvider { get; set; }
        public IClusterConfigClient ClusterConfigClient { get; set; }
    }
}