using JetBrains.Annotations;
using Vostok.ClusterConfig.Client.Abstractions;
using Vostok.Configuration.Abstractions;
using Vostok.Hosting.Abstractions;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokConfigurationContext
    {
        /// <inheritdoc cref="IVostokHostingEnvironment.ConfigurationSource"/>
        [NotNull]
        IConfigurationSource ConfigurationSource { get; }

        /// <inheritdoc cref="IVostokHostingEnvironment.SecretConfigurationSource"/>
        [NotNull]
        IConfigurationSource SecretConfigurationSource { get; }
        
        /// <summary>
        /// <para>A source of raw secret configuration parameters provided by the host system.</para>
        /// <para>Consists of combined sources of <see cref="ConfigurationSource"/> and <see cref="SecretConfigurationSource"/>.</para>
        /// <para>Use it in conjunction with <see cref="ConfigurationProvider"/>.</para>
        /// </summary>
        [NotNull]
        IConfigurationSource MergedConfigurationSource { get; }

        /// <inheritdoc cref="IVostokHostingEnvironment.ConfigurationProvider"/>
        [NotNull]
        IConfigurationProvider ConfigurationProvider { get; }

        /// <inheritdoc cref="IVostokHostingEnvironment.SecretConfigurationProvider"/>
        [NotNull]
        IConfigurationProvider SecretConfigurationProvider { get; }

        /// <inheritdoc cref="IVostokHostingEnvironment.ClusterConfigClient"/>
        [NotNull]
        IClusterConfigClient ClusterConfigClient { get; }
    }
}