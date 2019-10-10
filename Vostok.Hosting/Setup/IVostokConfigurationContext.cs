using JetBrains.Annotations;
using Vostok.ClusterConfig.Client.Abstractions;
using Vostok.Configuration.Abstractions;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokConfigurationContext
    {
        [NotNull]
        IConfigurationSource ConfigurationSource { get; }

        [NotNull]
        IConfigurationProvider ConfigurationProvider { get; }

        [NotNull]
        IClusterConfigClient ClusterConfigClient { get; }
    }
}