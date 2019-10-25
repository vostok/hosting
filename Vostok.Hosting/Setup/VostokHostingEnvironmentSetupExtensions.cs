using JetBrains.Annotations;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public static class VostokHostingEnvironmentSetupExtensions
    {
        public static IVostokCompositeLogBuilder SetupFileLog([NotNull] this IVostokCompositeLogBuilder builder) =>
            builder.SetupFileLog(_ => {});

        public static IVostokCompositeLogBuilder SetupConsoleLog([NotNull] this IVostokCompositeLogBuilder builder) =>
            builder.SetupConsoleLog(_ => {});

        public static IVostokHostingEnvironmentBuilder SetupServiceBeacon([NotNull] this IVostokHostingEnvironmentBuilder builder) =>
            builder.SetupServiceBeacon(_ => {});

        public static IVostokMetricsBuilder SetupHerculesMetricEventSender([NotNull] this IVostokMetricsBuilder builder) =>
            builder.SetupHerculesMetricEventSender(_ => {});
    }
}