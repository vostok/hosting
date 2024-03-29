﻿using Vostok.Logging.Abstractions;
using Vostok.Metrics;
using Vostok.Metrics.Models;

namespace Vostok.Hosting.Components.Metrics
{
    internal class LoggingMetricEventSender : IMetricEventSender, IAnnotationEventSender
    {
        private readonly ILog log;

        public LoggingMetricEventSender(ILog log)
            => this.log = log.ForContext("Metrics");

        public void Send(MetricEvent @event)
            => log.Debug("Metric event: {MetricEvent}", @event);

        public void Send(AnnotationEvent @event)
            => log.Debug("Annotation event: {AnnotationEvent}", @event);
    }
}