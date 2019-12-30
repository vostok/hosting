using System;
using System.Collections.Generic;
using System.Threading;
using Vostok.Commons.Time;
using Vostok.Hercules.Client;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hosting.Abstractions;
using Vostok.Metrics;
using Vostok.Metrics.Models;
using Vostok.Metrics.Scraping;

namespace Vostok.Hosting.Components.Hercules
{
    internal class HerculesSinkMetrics : IScrapableMetric
    {
        private static readonly TimeSpan ScrapePeriod = 1.Minutes();

        private readonly HerculesSink herculesSink;
        private readonly MetricTags tags;
        private volatile HerculesSinkCounters previous = HerculesSinkCounters.Zero;

        private HerculesSinkMetrics(IMetricContext context, HerculesSink herculesSink)
        {
            this.herculesSink = herculesSink;
            tags = context.Tags;

            context.Register(this, ScrapePeriod);
        }

        public static void Measure(IVostokApplicationMetrics context, IHerculesSink herculesSink)
        {
            if (!(herculesSink is HerculesSink sink))
                return;

            // ReSharper disable once ObjectCreationAsStatement
            // TODO(kungurtsev): what should be final paths?
            new HerculesSinkMetrics(context.Application.WithTag("owner", "ByHosting").WithTag("component", "HerculesSink"), sink);
        }

        public IEnumerable<MetricEvent> Scrape(DateTimeOffset timestamp)
        {
            var statistic = herculesSink.GetStatistics().Total;
            var delta = statistic - Interlocked.Exchange(ref previous, statistic);

            yield return CreateMetricEvent(timestamp, "RecordsLostDueToBuildFailures", delta.RecordsLostDueToBuildFailures);
            yield return CreateMetricEvent(timestamp, "RecordsLostDueToOverflows", delta.RecordsLostDueToOverflows);
            yield return CreateMetricEvent(timestamp, "RecordsLostDueToSizeLimit", delta.RecordsLostDueToSizeLimit);
            yield return CreateMetricEvent(timestamp, "TotalLostRecords", delta.TotalLostRecords);
            yield return CreateMetricEvent(timestamp, "RejectedRecordsCount", delta.RejectedRecords.Count);
            yield return CreateMetricEvent(timestamp, "RejectedRecordsSize", delta.RejectedRecords.Size);
            yield return CreateMetricEvent(timestamp, "SentRecordsSize", delta.SentRecords.Size);
            yield return CreateMetricEvent(timestamp, "SentRecordsCount", delta.SentRecords.Count);
            yield return CreateMetricEvent(timestamp, "StoredRecordsSize", statistic.StoredRecords.Size);
            yield return CreateMetricEvent(timestamp, "StoredRecordsCount", statistic.StoredRecords.Count);
            yield return CreateMetricEvent(timestamp, "Capacity", statistic.Capacity);
        }

        private MetricEvent CreateMetricEvent(DateTimeOffset timestamp, string name, double value)
        {
            return new MetricEvent(
                value,
                tags
                    .Append("summarized", "Total") //may be by stream
                    .Append(WellKnownTagKeys.Name, name),
                timestamp,
                null,
                WellKnownAggregationTypes.Counter,
                null);
        }
    }
}