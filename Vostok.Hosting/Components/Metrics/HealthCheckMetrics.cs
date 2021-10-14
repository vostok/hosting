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

        public static IDisposable Measure(IHealthTracker healthTracker, IVostokApplicationMetrics context)
        {
            var metrics = new HealthCheckMetrics(healthTracker, context.Instance.WithTag(WellKnownTagKeys.Component, "VostokHealthChecks"));
            return healthTracker.ObserveReports().Subscribe(metrics);
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(HealthReport value)
        {
            foreach (var check in value.Checks)
            {
                context.Send(
                    new MetricDataPoint(
                        Convert.ToDouble(check.Value.Status),
                        (WellKnownTagKeys.Name, "HealthCheckStatus"),
                        ("HealthCheckName", check.Key))
                    {
                        Timestamp = value.Timestamp
                    }
                );
            }
        }
    }
}