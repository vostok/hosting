using System;
using JetBrains.Annotations;
using Vostok.Metrics;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokMetricsBuilder
    {
        IVostokMetricsBuilder SetupHerculesMetricEventSender();
        IVostokMetricsBuilder SetupHerculesMetricEventSender([NotNull] Action<IVostokHerculesMetricEventSenderBuilder> herculesMetricEventSenderSetup);

        IVostokMetricsBuilder AddMetricEventSender([NotNull] IMetricEventSender metricEventSender);

        IVostokMetricsBuilder CustomizeSettings([NotNull] Action<MetricContextConfig> settingsCustomization);
    }
}