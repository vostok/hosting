using System;
using JetBrains.Annotations;
using Vostok.Metrics;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokMetricsBuilder
    {
        IVostokMetricsBuilder SetupHerculesMetricEventSender([NotNull] Action<IVostokHerculesMetricEventSenderBuilder> herculesMetricEventSenderSetup);

        IVostokMetricsBuilder AddMetricEventSender([NotNull] IMetricEventSender metricEventSender);
    }
}