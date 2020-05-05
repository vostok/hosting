using JetBrains.Annotations;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Sources;
using Vostok.Hosting.Abstractions;
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
        /// Enables ClusterConfig client in case it was disabled.
        /// </summary>
        public static IVostokHostingEnvironmentBuilder EnableClusterConfig([NotNull] this IVostokHostingEnvironmentBuilder builder) =>
            builder
                .EnableClusterConfigLocalSettings()
                .EnableClusterConfigRemoteSettings();

        /// <summary>
        /// Enables local settings for ClusterConfig client.
        /// </summary>
        public static IVostokHostingEnvironmentBuilder EnableClusterConfigLocalSettings([NotNull] this IVostokHostingEnvironmentBuilder builder) =>
            builder.SetupClusterConfigClient(
                clusterConfigBuilder => clusterConfigBuilder.CustomizeSettings(
                    settings => { settings.EnableLocalSettings = true; }));

        /// <summary>
        /// Enables remote (cluster) settings for ClusterConfig client.
        /// </summary>
        public static IVostokHostingEnvironmentBuilder EnableClusterConfigRemoteSettings([NotNull] this IVostokHostingEnvironmentBuilder builder) =>
            builder.SetupClusterConfigClient(
                clusterConfigBuilder => clusterConfigBuilder.CustomizeSettings(
                    settings => { settings.EnableClusterSettings = true; }));

        /// <summary>
        /// Disables Hercules telemetry altogether.
        /// </summary>
        public static IVostokHostingEnvironmentBuilder DisableHercules([NotNull] this IVostokHostingEnvironmentBuilder builder) =>
            builder.SetupHerculesSink(sink => sink.Disable());

        /// <summary>
        /// Disables ZooKeeper connection.
        /// </summary>
        public static IVostokHostingEnvironmentBuilder DisableZooKeeper([NotNull] this IVostokHostingEnvironmentBuilder builder) =>
            builder.SetupZooKeeperClient(zk => zk.Disable());

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
            builder.SetupServiceBeacon(beacon => beacon.SetupReplicaInfo(replica => replica.SetPort(port)));

        /// <summary>
        /// Applies given base url <paramref name="path"/> to <see cref="IServiceBeacon"/> configuration.
        /// </summary>
        public static IVostokHostingEnvironmentBuilder SetBaseUrlPath([NotNull] this IVostokHostingEnvironmentBuilder builder, [NotNull] string path) =>
            builder.SetupServiceBeacon(beacon => beacon.SetupReplicaInfo(replica => replica.SetUrlPath(path)));

        /// <summary>
        /// Applies given url <paramref name="scheme"/> to <see cref="IServiceBeacon"/> configuration.
        /// </summary>
        public static IVostokHostingEnvironmentBuilder SetUrlScheme([NotNull] this IVostokHostingEnvironmentBuilder builder, [NotNull] string scheme) =>
            builder.SetupServiceBeacon(beacon => beacon.SetupReplicaInfo(replica => replica.SetScheme(scheme)));

        /// <summary>
        /// Enables HTTPS scheme in <see cref="IServiceBeacon"/> configuration.
        /// </summary>
        public static IVostokHostingEnvironmentBuilder SetHttpsScheme([NotNull] this IVostokHostingEnvironmentBuilder builder) =>
            builder.SetUrlScheme("https");

        /// <summary>
        /// Applies given <paramref name="environment"/> name to <see cref="IServiceBeacon"/> configuration. Note the this environment might differ from app identity <see cref="IVostokApplicationIdentity.Environment"/>.
        /// </summary>
        public static IVostokHostingEnvironmentBuilder SetBeaconEnvironment([NotNull] this IVostokHostingEnvironmentBuilder builder, [NotNull] string environment) =>
            builder.SetupServiceBeacon(beacon => beacon.SetupReplicaInfo(replica => replica.SetEnvironment(environment)));

        /// <summary>
        /// Applies given <paramref name="application"/> name to <see cref="IServiceBeacon"/> configuration. Note the this application might differ from app identity <see cref="IVostokApplicationIdentity.Application"/>.
        /// </summary>
        public static IVostokHostingEnvironmentBuilder SetBeaconApplication([NotNull] this IVostokHostingEnvironmentBuilder builder, [NotNull] string application) =>
            builder.SetupServiceBeacon(beacon => beacon.SetupReplicaInfo(replica => replica.SetApplication(application)));

        /// <summary>
        /// Adds <see cref="LogConfigurationRule"/>s from built-in <see cref="IConfigurationSource"/> in given <paramref name="scope"/>.
        /// </summary>
        public static IVostokHostingEnvironmentBuilder AddLoggingRulesFromSettings([NotNull] this IVostokHostingEnvironmentBuilder builder, [NotNull] params string[] scope) =>
            builder.SetupLog((log, context) => log.AddRules(context.ConfigurationSource.ScopeTo(scope)));
    }
}
