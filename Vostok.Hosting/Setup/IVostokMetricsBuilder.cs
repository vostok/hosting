using System;
using JetBrains.Annotations;
using Vostok.Metrics;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokMetricsBuilder
    {
        IVostokMetricsBuilder SetupHerculesMetricEventSender([NotNull] Action<IVostokHerculesMetricEventSenderBuilder> herculesMetricEventSenderSetup);

        IVostokMetricsBuilder SetupLoggingMetricEventSender();

        IVostokMetricsBuilder AddMetricEventSender([NotNull] IMetricEventSender metricEventSender);

        IVostokMetricsBuilder CustomizeSettings([NotNull] Action<MetricContextConfig> settingsCustomization);
    }
}