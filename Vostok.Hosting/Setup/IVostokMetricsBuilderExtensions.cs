using System.Linq;
using JetBrains.Annotations;
using Vostok.Metrics.Models;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public static class IVostokMetricsBuilderExtensions
    {
        /// <summary>
        /// Enables sending metrics to hercules.
        /// </summary>
        public static IVostokMetricsBuilder SetupHerculesMetricEventSender([NotNull] this IVostokMetricsBuilder builder) =>
            builder.SetupHerculesMetricEventSender(b => b.Enable());

        /// <inheritdoc cref="IVostokMetricsBuilder.EnrichAnnotationTags" />
        public static IVostokMetricsBuilder EnrichAnnotationTags([NotNull] this IVostokMetricsBuilder builder, [NotNull] params (string key, string value)[] tags)
        {
            var metricTags = tags.Select(t => new MetricTag(t.key, t.value)).ToArray();
            return builder.EnrichAnnotationTags(new MetricTags(metricTags));
        }
    }
}