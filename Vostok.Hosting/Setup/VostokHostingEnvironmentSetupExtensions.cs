using JetBrains.Annotations;
using Vostok.ServiceDiscovery.Abstractions;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public static class VostokHostingEnvironmentSetupExtensions
    {
        /// <summary>
        /// Enables file log.
        /// </summary>
        public static IVostokCompositeLogBuilder SetupFileLog([NotNull] this IVostokCompositeLogBuilder builder) =>
            builder.SetupFileLog(_ => {});

        /// <summary>
        /// Enables console log.
        /// </summary>
        public static IVostokCompositeLogBuilder SetupConsoleLog([NotNull] this IVostokCompositeLogBuilder builder) =>
            builder.SetupConsoleLog(_ => {});

        /// <summary>
        /// Enables service beacon.
        /// </summary>
        public static IVostokHostingEnvironmentBuilder SetupServiceBeacon([NotNull] this IVostokHostingEnvironmentBuilder builder) =>
            builder.SetupServiceBeacon(_ => {});

        /// <summary>
        /// Enables sending metrics to hercules.
        /// </summary>
        public static IVostokMetricsBuilder SetupHerculesMetricEventSender([NotNull] this IVostokMetricsBuilder builder) =>
            builder.SetupHerculesMetricEventSender(_ => {});

        /// <summary>
        /// Sets given <paramref name="port"/> to <see cref="IServiceBeacon"/>.
        /// </summary>
        public static IVostokHostingEnvironmentBuilder SetPort([NotNull] this IVostokHostingEnvironmentBuilder builder, int port) =>
            builder.SetupServiceBeacon(serviceBeaconSetup => serviceBeaconSetup.SetupReplicaInfo(replicaInfoSetup => replicaInfoSetup.SetPort(port)));
    }
}