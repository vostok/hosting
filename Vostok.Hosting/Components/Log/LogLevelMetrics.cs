using System;
using System.Collections.Generic;
using Vostok.Commons.Time;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Abstractions;
using Vostok.Metrics;
using Vostok.Metrics.Models;
using Vostok.Metrics.Scraping;

namespace Vostok.Hosting.Components.Log
{
    internal class LogLevelMetrics : IScrapableMetric
    {
        private static readonly TimeSpan ScrapePeriod = 1.Minutes();
        private readonly EventLevelCounter counter;
        private readonly MetricTags tags;
        private readonly ILog log;

        private volatile LogLevelStatistics previousStatistics = LogLevelStatistics.Zero;

        public LogLevelMetrics(EventLevelCounter counter, IMetricContext context, ILog log)
        {
            this.counter = counter;
            tags = context.Tags;
            this.log = log;

            context.Register(this, ScrapePeriod);
        }

        public static void Measure(EventLevelCounter counter, IVostokApplicationMetrics context, ILog log)
        {
            // ReSharper disable once ObjectCreationAsStatement
            new LogLevelMetrics(counter, context.Application.WithTag(WellKnownTagKeys.Component, "LogLevel"), log);
        }

        public IEnumerable<MetricEvent> Scrape(DateTimeOffset timestamp)
        {
            var statistics = counter.CollectStatistics();

            foreach (var property in typeof(LogLevelStatistics).GetProperties())
                yield return CreateMetricEvent(
                    timestamp,
                    property.Name,
                    (int)property.GetValue(statistics) - (int)property.GetValue(previousStatistics));

            previousStatistics = statistics;
        }

        private MetricEvent CreateMetricEvent(DateTimeOffset timestamp, string name, double value) // TODO: Double check
        {
            return new MetricEvent(
                value,
                tags.Append(WellKnownTagKeys.Name, name),
                timestamp,
                null,
                WellKnownAggregationTypes.Counter,
                null);
        }
    }
}