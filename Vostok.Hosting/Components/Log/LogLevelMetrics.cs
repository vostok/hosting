using System;
using System.Collections.Generic;
using Vostok.Hosting.Abstractions;
using Vostok.Metrics;
using Vostok.Metrics.Models;
using Vostok.Metrics.Primitives.Gauge;

namespace Vostok.Hosting.Components.Log
{
    internal class LogLevelMetrics
    {
        private readonly LogEventLevelCounter counter;

        public LogLevelMetrics(LogEventLevelCounter counter, IMetricContext context)
        {
            this.counter = counter;

            context.CreateMultiFuncGauge(ProvideMetrics, new FuncGaugeConfig {ScrapeOnDispose = true});
        }

        public static void Measure(LogEventLevelCounter counter, IVostokApplicationMetrics context)
        {
            // ReSharper disable once ObjectCreationAsStatement
            new LogLevelMetrics(
                counter,
                context.Instance
                   .WithTag(WellKnownTagKeys.Component, "VostokLog")
                   .WithTag(WellKnownTagKeys.Name, "EventsByLevel"));
        }

        private IEnumerable<MetricDataPoint> ProvideMetrics()
        {
            var metrics = counter.Collect();

            foreach (var property in typeof(LogEventsMetrics).GetProperties())
                yield return new MetricDataPoint(
                    Convert.ToDouble(property.GetValue(metrics)),
                    ("LogLevel", property.Name.Replace("LogEvents", string.Empty)));
        }
    }
}