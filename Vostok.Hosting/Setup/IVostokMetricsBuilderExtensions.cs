using JetBrains.Annotations;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public static class IVostokMetricsBuilderExtensions
    {
        /// <summary>
        /// Enables sending metrics to hercules.
        /// </summary>
        public static IVostokMetricsBuilder SetupHerculesMetricEventSender([NotNull] this IVostokMetricsBuilder builder) =>
            builder.SetupHerculesMetricEventSender(_ => { });
    }
}
