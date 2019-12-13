using JetBrains.Annotations;
using Vostok.ClusterConfig.Client.Abstractions;
using Vostok.Configuration.Abstractions;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.Setup
{
    /// <summary>
    /// Provides a way to use log and configuration during environment setup.
    /// </summary>
    [PublicAPI]
    public interface IVostokHostingEnvironmentSetupContext
    {
        /// <inheritdoc cref="IVostokHostingEnvironment.Log"/>
        [NotNull]
        ILog Log { get; }

        /// <inheritdoc cref="IVostokHostingEnvironment.ConfigurationSource"/>
        [NotNull]
        IConfigurationSource ConfigurationSource { get; }

        /// <inheritdoc cref="IVostokHostingEnvironment.SecretConfigurationSource"/>
        [NotNull]
        IConfigurationSource SecretConfigurationSource { get; }

        /// <inheritdoc cref="IVostokHostingEnvironment.ConfigurationProvider"/>
        [NotNull]
        IConfigurationProvider ConfigurationProvider { get; }

        /// <inheritdoc cref="IVostokHostingEnvironment.ClusterConfigClient"/>
        [NotNull]
        IClusterConfigClient ClusterConfigClient { get; }
    }
}