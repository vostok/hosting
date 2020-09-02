using System;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Diagnostics;
using Vostok.Logging.Abstractions;
using Vostok.Metrics;
using Vostok.Metrics.Models;

namespace Vostok.Hosting.Components.Metrics
{
    internal class HealthCheckMetrics : IObserver<HealthReport>
    {
        private readonly ILog log;
        private readonly IMetricContext context;

        private HealthCheckMetrics(IHealthTracker healthTracker, IMetricContext context, ILog log)
        {
            this.log = log.ForContext<IHealthTracker>();
            this.context = context;

            healthTracker.ObserveReports().Subscribe(this);
        }

        public static void Measure(IHealthTracker healthTracker, IVostokApplicationMetrics context, ILog log)
        {
            // ReSharper disable once ObjectCreationAsStatement
            new HealthCheckMetrics(healthTracker, context.Instance.WithTag(WellKnownTagKeys.Component, "HealthCheck"), log);
        }

        public void OnCompleted() {}

        public void OnError(Exception error) {}

        public void OnNext(HealthReport value)
        {
            foreach (var keyValuePair in value.Checks)
            {
                context.Send(
                    new MetricDataPoint(
                        Convert.ToDouble(keyValuePair.Value.Reason),
                        (WellKnownTagKeys.Name, keyValuePair.Key)
                    ));
            }
            log.Info("Successfully sent health check metrics.");
        }
    }
}