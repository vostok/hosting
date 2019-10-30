using JetBrains.Annotations;
using Vostok.ClusterConfig.Client.Abstractions;
using Vostok.Configuration.Abstractions;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokConfigurationContext
    {
        /// <inheritdoc cref="IConfigurationSource"/>
        [NotNull]
        IConfigurationSource ConfigurationSource { get; }

        /// <inheritdoc cref="IConfigurationProvider"/>
        [NotNull]
        IConfigurationProvider ConfigurationProvider { get; }

        /// <inheritdoc cref="IClusterConfigClient"/>
        [NotNull]
        IClusterConfigClient ClusterConfigClient { get; }
    }
}