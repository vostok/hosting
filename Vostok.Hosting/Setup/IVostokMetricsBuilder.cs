using System;
using JetBrains.Annotations;
using Vostok.Metrics;
using Vostok.Metrics.Models;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokMetricsBuilder
    {
        IVostokMetricsBuilder SetupHerculesMetricEventSender([NotNull] Action<IVostokHerculesMetricEventSenderBuilder> herculesMetricEventSenderSetup);

        IVostokMetricsBuilder SetupLoggingMetricEventSender();

        IVostokMetricsBuilder AddMetricEventSender([NotNull] IMetricEventSender metricEventSender);

        IVostokMetricsBuilder CustomizeSettings([NotNull] Action<MetricContextConfig> settingsCustomization);

        /// <summary>
        /// Applies given <paramref name="tags" /> to all annotations.
        /// </summary>
        IVostokMetricsBuilder EnrichAnnotationTags([NotNull] MetricTags tags);
    }
}