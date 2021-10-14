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

        private LogLevelMetrics(LogEventLevelCounter counter)
        {
            this.counter = counter;
        }

        public static IDisposable Measure(LogEventLevelCounter counter, IVostokApplicationMetrics context)
        {
            return context.Instance
                   .WithTag(WellKnownTagKeys.Component, "VostokLog")
                   .WithTag(WellKnownTagKeys.Name, "EventsByLevel")
                   .CreateMultiFuncGauge(new LogLevelMetrics(counter).ProvideMetrics, new FuncGaugeConfig {ScrapeOnDispose = true})
                as IDisposable;
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