using System;
using JetBrains.Annotations;
using Vostok.ClusterConfig.Client;
using Vostok.Configuration;
using Vostok.Datacenters;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Abstractions;
using Vostok.Tracing.Abstractions;

namespace Vostok.Hosting.Helpers
{
    /// <summary>
    /// See <see cref="Configure"/>.
    /// </summary>
    [PublicAPI]
    public static class StaticProvidersHelper
    {
        /// <summary>
        /// <para>Configures common static providers with values from given <paramref name="environment"/>.</para>
        /// <para>Following static providers are configured:</para>
        /// <list type="bullet">
        ///     <item><description><see cref="LogProvider"/></description></item>
        ///     <item><description><see cref="TracerProvider"/></description></item>
        ///     <item><description><see cref="HerculesSinkProvider"/></description></item>
        ///     <item><description><see cref="DatacentersProvider"/></description></item>
        ///     <item><description>ClusterConfigClient.<see cref="ClusterConfigClient.Default"/> (if not configured earlier)</description></item>
        ///     <item><description>ConfigurationProvider.<see cref="ConfigurationProvider.Default"/> (if not configured earlier)</description></item>
        /// </list>
        /// </summary>
        public static void Configure([NotNull] IVostokHostingEnvironment environment)
        {
            if (environment == null)
                throw new ArgumentNullException(nameof(environment));

            LogProvider.Configure(environment.Log, true);
            TracerProvider.Configure(environment.Tracer, true);
            HerculesSinkProvider.Configure(environment.HerculesSink, true);
            DatacentersProvider.Configure(environment.Datacenters, true);

            var log = environment.Log.ForContext(typeof(StaticProvidersHelper));

            if (environment.ClusterConfigClient is ClusterConfigClient clusterConfigClient)
            {
                if (!ClusterConfigClient.TrySetDefaultClient(clusterConfigClient))
                    log.Warn("ClusterConfigClient.Default has already been configured.");
            }

            if (environment.ConfigurationProvider is ConfigurationProvider configurationProvider)
            {
                if (!ConfigurationProvider.TrySetDefault(configurationProvider))
                    log.Warn("ConfigurationProvider.Default has already been configured.");
            }
        }
    }
}