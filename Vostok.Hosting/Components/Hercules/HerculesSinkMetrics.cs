using System;
using System.Collections.Generic;
using System.Text;
using Vostok.Hercules.Client;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Abstractions;
using Vostok.Metrics;
using Vostok.Metrics.Models;
using Vostok.Metrics.Primitives.Gauge;

namespace Vostok.Hosting.Components.Hercules
{
    internal class HerculesSinkMetrics
    {
        private readonly ILog log;
        private readonly HerculesSink herculesSink;
        private readonly MetricTags tags;

        private volatile HerculesSinkStatistics previous = new HerculesSinkStatistics(HerculesSinkCounters.Zero, new Dictionary<string, HerculesSinkCounters>());

        private HerculesSinkMetrics(HerculesSink herculesSink, IMetricContext context, ILog log)
        {
            this.log = log.ForContext<HerculesSink>();
            this.herculesSink = herculesSink;
            tags = context.Tags;
        }

        public static IDisposable Measure(IHerculesSink herculesSink, IVostokApplicationMetrics context, ILog log)
        {
            if (!(herculesSink is HerculesSink sink))
                return null;

            var builtContext = context.Application.WithTag(WellKnownTagKeys.Component, "HerculesSink");
            return builtContext.CreateMultiFuncGaugeFromEvents(new HerculesSinkMetrics(sink, builtContext, log).ProvideMetrics, new FuncGaugeConfig(){ScrapeOnDispose = true}) as IDisposable;
        }

        public IEnumerable<MetricEvent> ProvideMetrics()
        {
            var timestamp = DateTimeOffset.Now;

            var statistic = herculesSink.GetStatistics();

            var delta = statistic.Total - previous.Total;

            LogSentRecords(statistic, delta);

            previous = statistic;

            yield return CreateMetricEvent(timestamp, "RecordsLostDueToBuildFailures", delta.RecordsLostDueToBuildFailures);
            yield return CreateMetricEvent(timestamp, "RecordsLostDueToOverflows", delta.RecordsLostDueToOverflows);
            yield return CreateMetricEvent(timestamp, "RecordsLostDueToSizeLimit", delta.RecordsLostDueToSizeLimit);
            yield return CreateMetricEvent(timestamp, "TotalLostRecords", delta.TotalLostRecords);
            yield return CreateMetricEvent(timestamp, "RejectedRecordsCount", delta.RejectedRecords.Count);
            yield return CreateMetricEvent(timestamp, "RejectedRecordsSize", delta.RejectedRecords.Size);
            yield return CreateMetricEvent(timestamp, "SentRecordsSize", delta.SentRecords.Size);
            yield return CreateMetricEvent(timestamp, "SentRecordsCount", delta.SentRecords.Count);
            yield return CreateMetricEvent(timestamp, "StoredRecordsSize", statistic.Total.StoredRecords.Size);
            yield return CreateMetricEvent(timestamp, "StoredRecordsCount", statistic.Total.StoredRecords.Count);
            yield return CreateMetricEvent(timestamp, "Capacity", statistic.Total.Capacity);
        }

        private void LogSentRecords(HerculesSinkStatistics statistic, HerculesSinkCounters delta)
        {
            var message = new StringBuilder("Successfully sent {RecordsCount} record(s) of size {RecordsSize}");
            var started = false;
            foreach (var kvp in statistic.PerStream)
            {
                previous.PerStream.TryGetValue(kvp.Key, out var p);
                var count = kvp.Value.SentRecords.Count - (p?.SentRecords.Count ?? 0);
                if (count == 0)
                    continue;

                message.Append(started ? ", " : " (");
                message.Append($"{count} {kvp.Key}");
                started = true;
            }

            message.Append(started ? ")." : ".");

            log.Info(message.ToString(), delta.SentRecords.Count, delta.SentRecords.Size);
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