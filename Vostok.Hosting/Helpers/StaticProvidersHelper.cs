using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core;
using Vostok.ClusterConfig.Client;
using Vostok.Configuration;
using Vostok.Context;
using Vostok.Datacenters;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Abstractions;
using Vostok.Metrics;
using Vostok.ServiceDiscovery;
using Vostok.ServiceDiscovery.Telemetry;
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
        ///     <item><description><see cref="MetricContextProvider"/></description></item>
        ///     <item><description><see cref="ServiceDiscoveryEventsContextProvider"/></description></item>
        ///     <item><description>ClusterConfigClient.<see cref="ClusterConfigClient.Default"/> (if not configured earlier)</description></item>
        ///     <item><description>ConfigurationProvider.<see cref="ConfigurationProvider.Default"/> (if not configured earlier)</description></item>
        /// </list>
        /// </summary>
        public static void Configure([NotNull] IVostokHostingEnvironment environment)
        {
            if (environment == null)
                throw new ArgumentNullException(nameof(environment));

            ClusterClientDefaults.ClientApplicationName = environment.ServiceBeacon is ServiceBeacon beacon
                ? beacon.ReplicaInfo.Application
                : environment.ApplicationIdentity.FormatServiceName();

            LogProvider.Configure(environment.Log, true);
            TracerProvider.Configure(environment.Tracer, true);
            HerculesSinkProvider.Configure(environment.HerculesSink, true);
            DatacentersProvider.Configure(environment.Datacenters, true);
            MetricContextProvider.Configure(environment.Metrics.Root, true);

            FlowingContext.Configuration.ErrorCallback = (errorMessage, error) => environment.Log.ForContext(typeof(FlowingContext)).Error(error, errorMessage);

            var log = environment.Log.ForContext(typeof(StaticProvidersHelper));

            if (environment.ClusterConfigClient is ClusterConfigClient clusterConfigClient)
            {
                if (!ClusterConfigClient.TrySetDefaultClient(clusterConfigClient) && !ReferenceEquals(ClusterConfigClient.Default, environment.ClusterConfigClient))
                    log.Warn("ClusterConfigClient.Default has already been configured.");
            }

            if (environment.ConfigurationProvider is ConfigurationProvider configurationProvider && !ReferenceEquals(ConfigurationProvider.Default, environment.ConfigurationProvider))
            {
                if (!ConfigurationProvider.TrySetDefault(configurationProvider))
                    log.Warn("ConfigurationProvider.Default has already been configured.");
            }

            if (environment.HostExtensions.TryGet<IServiceDiscoveryEventsContext>(out var context))
            {
                ServiceDiscoveryEventsContextProvider.Configure(context, true);
            }
        }
    }
}