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
        private readonly LogEventLevelCounter counter;

        public LogLevelMetrics(LogEventLevelCounter counter, IMetricContext context)
        {
            this.counter = counter;
            
            context.CreateMultiFuncGauge(ProvideMetrics);
        }

        public static void Measure(LogEventLevelCounter counter, IVostokApplicationMetrics context)
        {
            // ReSharper disable once ObjectCreationAsStatement
            new LogLevelMetrics(counter, context.Instance.WithTag(WellKnownTagKeys.Component, "LogLevel"));
        }

        private IEnumerable<MetricDataPoint> ProvideMetrics()
        {
            var metrics = counter.Collect();

            foreach (var property in typeof(LogEventsMetrics).GetProperties())
                yield return new MetricDataPoint(
                    Convert.ToDouble(property.GetValue(metrics)),
                    (WellKnownTagKeys.Name, property.Name));
        }
    }
}