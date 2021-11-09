using System.Collections.Generic;
using System.Linq;
using Vostok.Commons.Collections;
using Vostok.Hosting.Abstractions.Diagnostics;
using Vostok.Metrics;
using Vostok.Metrics.Models;

namespace Vostok.Hosting.Components.Diagnostics.InfoProviders
{
    internal class ApplicationMetricsProvider : IDiagnosticInfoProvider, IMetricEventSender
    {
        private const int BufferCapacity = 1000;

        private static readonly HashSet<string> AllowedAggregationTypes = new HashSet<string>
        {
            WellKnownAggregationTypes.Counter
        };

        private readonly CircularBuffer<MetricEvent> events = new CircularBuffer<MetricEvent>(BufferCapacity);
        private readonly object sync = new object();

        public void Send(MetricEvent @event)
        {
            if (!string.IsNullOrEmpty(@event.AggregationType) && !AllowedAggregationTypes.Contains(@event.AggregationType))
                return;

            lock (sync)
                events.Add(@event);
        }

        public object Query()
        {
            MetricEvent[] snapshot;

            lock (sync)
                snapshot = events.ToArray();

            var result = new Dictionary<string, double>();

            foreach (var @event in snapshot)
                result[GetFlatMetricName(@event)] = @event.Value;

            return result.OrderBy(pair => pair.Key).ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        private static string GetFlatMetricName(MetricEvent @event)
            => string.Join(".", @event.Tags.Select(tag => tag.Value));
    }
}