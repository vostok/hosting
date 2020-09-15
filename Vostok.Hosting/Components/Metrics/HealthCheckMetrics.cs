using System;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Diagnostics;
using Vostok.Metrics;
using Vostok.Metrics.Models;

namespace Vostok.Hosting.Components.Metrics
{
    internal class HealthCheckMetrics : IObserver<HealthReport>
    {
        private readonly IMetricContext context;

        private HealthCheckMetrics(IHealthTracker healthTracker, IMetricContext context)
        {
            this.context = context;

            healthTracker.ObserveReports().Subscribe(this);
        }

        public static void Measure(IHealthTracker healthTracker, IVostokApplicationMetrics context)
        {
            // ReSharper disable once ObjectCreationAsStatement
            new HealthCheckMetrics(healthTracker, context.Instance.WithTag(WellKnownTagKeys.Component, "VostokHealthChecks"));
        }

        public void OnCompleted() {}

        public void OnError(Exception error) {}

        public void OnNext(HealthReport value)
        {
            foreach (var keyValuePair in value.Checks)
            {
                context.Send(
                    new MetricDataPoint(
                        Convert.ToDouble(keyValuePair.Value.Status),
                        (WellKnownTagKeys.Name, "HealthCheckStatus"),
                        ("HealthCheckName", keyValuePair.Key)
                    )
                );
            }
        }
    }
}