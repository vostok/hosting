using System;
using System.Collections.Generic;
using System.Reflection;
using Vostok.Hosting.Abstractions;
using Vostok.Metrics;
using Vostok.Metrics.Models;
using Vostok.Metrics.Primitives.Gauge;

namespace Vostok.Hosting.Components.Log
{
    internal class LogLevelMetrics
    {
        private readonly IMetricContext context;
        private readonly LogEventLevelCounter counter;

        public LogLevelMetrics(LogEventLevelCounter counter, IMetricContext context)
        {
            this.context = context;
            this.counter = counter;

            context.CreateMultiFuncGaugeFromEvents(ProvideMetrics, new FuncGaugeConfig {ScrapeOnDispose = true});
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

        private IEnumerable<MetricEvent> ProvideMetrics()
        {
            MetricEvent LogEventToMetricEvent(PropertyInfo logEventInfo, LogEventsMetrics logEventsMetrics, DateTimeOffset timestamp)
            {
                return new MetricEvent(
                    Convert.ToDouble(logEventInfo.GetValue(logEventsMetrics)),
                    MetricTagsMerger.Merge(context.Tags,
                        null,
                        new[] {new MetricTag("LogLevel", logEventInfo.Name.Replace("LogEvents", string.Empty))}),
                    timestamp,
                    WellKnownUnits.None,
                    WellKnownAggregationTypes.Counter,
                    null
                );
            }

            var metrics = counter.Collect();
            var now = DateTimeOffset.Now;

            foreach (var property in typeof(LogEventsMetrics).GetProperties())
                yield return LogEventToMetricEvent(property, metrics, now);
        }
    }
}