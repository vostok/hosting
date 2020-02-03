using JetBrains.Annotations;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Sources;
using Vostok.Logging.Configuration;
using Vostok.ServiceDiscovery.Abstractions;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public static class IVostokHostingEnvironmentBuilderExtensions
    {
        /// <summary>
        /// Disables ClusterConfig client altogether.
        /// </summary>
        public static IVostokHostingEnvironmentBuilder DisableClusterConfig([NotNull] this IVostokHostingEnvironmentBuilder builder) =>
            builder
                .DisableClusterConfigLocalSettings()
                .DisableClusterConfigRemoteSettings();

        /// <summary>
        /// Disables local settings for ClusterConfig client.
        /// </summary>
        public static IVostokHostingEnvironmentBuilder DisableClusterConfigLocalSettings([NotNull] this IVostokHostingEnvironmentBuilder builder) =>
            builder.SetupClusterConfigClient(
                clusterConfigBuilder => clusterConfigBuilder.CustomizeSettings(
                    settings => { settings.EnableLocalSettings = false; }));

        /// <summary>
        /// Disables remote (cluster) settings for ClusterConfig client.
        /// </summary>
        public static IVostokHostingEnvironmentBuilder DisableClusterConfigRemoteSettings([NotNull] this IVostokHostingEnvironmentBuilder builder) =>
            builder.SetupClusterConfigClient(
                clusterConfigBuilder => clusterConfigBuilder.CustomizeSettings(
                    settings => { settings.EnableClusterSettings = false; }));

        /// <summary>
        /// Disables Hercules telemetry altogether.
        /// </summary>
        public static IVostokHostingEnvironmentBuilder DisableHercules([NotNull] this IVostokHostingEnvironmentBuilder builder) =>
            builder.SetupHerculesSink(sink => sink.Disable());

        /// <summary>
        /// Disables service beacon.
        /// </summary>
        public static IVostokHostingEnvironmentBuilder DisableServiceBeacon([NotNull] this IVostokHostingEnvironmentBuilder builder) =>
            builder.SetupServiceBeacon(serviceBeaconSetup => serviceBeaconSetup.Disable());

        /// <summary>
        /// Enables service beacon.
        /// </summary>
        public static IVostokHostingEnvironmentBuilder SetupServiceBeacon([NotNull] this IVostokHostingEnvironmentBuilder builder) =>
            builder.SetupServiceBeacon(_ => {});

        /// <summary>
        /// Applies given <paramref name="port"/> to <see cref="IServiceBeacon"/> configuration.
        /// </summary>
        public static IVostokHostingEnvironmentBuilder SetPort([NotNull] this IVostokHostingEnvironmentBuilder builder, int port) =>
            builder.SetupServiceBeacon(serviceBeaconSetup => serviceBeaconSetup.SetupReplicaInfo(replicaInfoSetup => replicaInfoSetup.SetPort(port)));

        /// <summary>
        /// Applies given base url <paramref name="path"/> to <see cref="IServiceBeacon"/> configuration.
        /// </summary>
        public static IVostokHostingEnvironmentBuilder SetBaseUrlPath([NotNull] this IVostokHostingEnvironmentBuilder builder, [NotNull] string path) =>
            builder.SetupServiceBeacon(serviceBeaconSetup => serviceBeaconSetup.SetupReplicaInfo(replicaInfoSetup => replicaInfoSetup.SetUrlPath(path)));

        /// <summary>
        /// Adds <see cref="LogConfigurationRule"/>s from built-in <see cref="IConfigurationSource"/> in given <paramref name="scope"/>.
        /// </summary>
        public static IVostokHostingEnvironmentBuilder AddLoggingRulesFromSettings([NotNull] this IVostokHostingEnvironmentBuilder builder, [NotNull] params string[] scope) =>
            builder.SetupLog((log, context) => log.AddRules(context.ConfigurationSource.ScopeTo(scope)));
    }
}
