using JetBrains.Annotations;
using Vostok.ServiceDiscovery.Abstractions;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public static class IVostokHostingEnvironmentBuilderExtensions
    {
        /// <summary>
        /// Enables service beacon.
        /// </summary>
        public static IVostokHostingEnvironmentBuilder SetupServiceBeacon([NotNull] this IVostokHostingEnvironmentBuilder builder) =>
            builder.SetupServiceBeacon(_ => {});

        /// <summary>
        /// Enables service beacon.
        /// </summary>
        public static IVostokHostingEnvironmentBuilder DisableServiceBeacon([NotNull] this IVostokHostingEnvironmentBuilder builder) =>
            builder.SetupServiceBeacon(serviceBeaconSetup => serviceBeaconSetup.Disable());

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
    }
}