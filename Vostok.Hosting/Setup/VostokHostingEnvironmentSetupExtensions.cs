using JetBrains.Annotations;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public static class VostokHostingEnvironmentSetupExtensions
    {
        public static IVostokCompositeLogBuilder SetupFileLog(this IVostokCompositeLogBuilder builder) =>
            builder.SetupFileLog(_ => {});

        public static IVostokCompositeLogBuilder SetupConsoleLog(this IVostokCompositeLogBuilder builder) =>
            builder.SetupConsoleLog(_ => {});

        public static IVostokHostingEnvironmentBuilder SetupServiceBeacon(this IVostokHostingEnvironmentBuilder builder) =>
            builder.SetupServiceBeacon(_ => {});

        public static IVostokMetricsBuilder SetupHerculesMetricEventSender(this IVostokMetricsBuilder builder) =>
            builder.SetupHerculesMetricEventSender(_ => {});
    }
}