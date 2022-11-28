using System.Linq;
using JetBrains.Annotations;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Components.Metrics;
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

        /// <inheritdoc cref="EnrichInstanceAnnotationTags(Vostok.Hosting.Setup.IVostokMetricsBuilder,Vostok.Metrics.Models.MetricTags)" />
        public static IVostokMetricsBuilder EnrichInstanceAnnotationTags([NotNull] this IVostokMetricsBuilder builder, [NotNull] params (string key, string value)[] tags)
        {
            var metricTags = tags.Select(t => new MetricTag(t.key, t.value)).ToArray();
            return builder.CustomizeAnnotationEventSender(sender
                => new TagsEnrichingInstanceAnnotationEventSender(sender, new MetricTags(metricTags)));
        }

        /// <summary>
        /// Applies given <paramref name="tags"/> to all annotations written by <see cref="IVostokApplicationMetrics.Instance"/> metrics context. 
        /// </summary>
        public static IVostokMetricsBuilder EnrichInstanceAnnotationTags([NotNull] this IVostokMetricsBuilder builder, [NotNull] MetricTags tags) =>
            builder.CustomizeAnnotationEventSender(sender => new TagsEnrichingInstanceAnnotationEventSender(sender, tags));
    }
}