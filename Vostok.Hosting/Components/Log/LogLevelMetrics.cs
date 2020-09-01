using System;
using System.Collections.Generic;
using Vostok.Commons.Time;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Abstractions;
using Vostok.Metrics;
using Vostok.Metrics.Models;
using Vostok.Metrics.Primitives.Gauge;

namespace Vostok.Hosting.Components.Log
{
    internal class LogLevelMetrics
    {
        private static readonly TimeSpan ScrapePeriod = 1.Minutes();
        private readonly EventLevelCounter counter;
        private readonly ILog log;

        public LogLevelMetrics(EventLevelCounter counter, IMetricContext context, ILog log)
        {
            context.CreateMultiFuncGauge(ProvideMetrics, new FuncGaugeConfig {ScrapePeriod = ScrapePeriod});

            this.counter = counter;
            this.log = log.ForContext<LogEventsMetrics>();
        }

        public static void Measure(EventLevelCounter counter, IVostokApplicationMetrics context, ILog log)
        {
            // ReSharper disable once ObjectCreationAsStatement
            new LogLevelMetrics(counter, context.Instance.WithTag(WellKnownTagKeys.Component, "LogLevel"), log);
        }

        private IEnumerable<MetricDataPoint> ProvideMetrics()
        {
            var metrics = counter.Collect();

            foreach (var property in typeof(LogEventsMetrics).GetProperties())
                yield return new MetricDataPoint(
                    Convert.ToDouble(property.GetValue(metrics)),
                    (WellKnownTagKeys.Name, property.Name));

            log.Info("Successfully sent log level metrics.");
        }
    }
}