using System;
using System.Linq;
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
            new HealthCheckMetrics(healthTracker, context.Instance.WithTag(WellKnownTagKeys.Component, "HealthCheck"));
        }

        public void OnCompleted() {}

        public void OnError(Exception error) {}

        public void OnNext(HealthReport value)
        {
            foreach (var keyValuePair in value.Checks.Where(x => x.Value.Status != HealthStatus.Healthy))
            {
                context.Send(
                    new MetricDataPoint(
                        Convert.ToDouble(keyValuePair.Value.Status),
                        (WellKnownTagKeys.Name, keyValuePair.Key)
                    ));
            }
        }
    }
}