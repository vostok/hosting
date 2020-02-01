using JetBrains.Annotations;
using Vostok.Logging.File.Configuration;
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
            builder.SetupFileLog(
                fileLogBuilder =>
                {
                    fileLogBuilder.CustomizeSettings(
                        settings =>
                        {
                            settings.RollingStrategy.Type = RollingStrategyType.ByTime;
                            settings.RollingStrategy.Period = RollingPeriod.Day;
                        });
                });

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