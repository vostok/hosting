using System;
using System.Collections.Generic;
using System.Linq;
using Vostok.Tracing.Abstractions;

namespace Vostok.Hosting.Components.Tracing
{
    internal class Span : ISpan
    {
        public Guid TraceId { get; }
        public Guid SpanId { get; }
        public Guid? ParentSpanId { get; }
        public DateTimeOffset BeginTimestamp { get; }
        public DateTimeOffset? EndTimestamp { get; }
        public IReadOnlyDictionary<string, object> Annotations => annotations;

        private Dictionary<string, object> annotations;

        public Span(ISpan span)
        {
            TraceId = span.TraceId;
            SpanId = span.SpanId;
            ParentSpanId = span.ParentSpanId;
            BeginTimestamp = span.BeginTimestamp;
            EndTimestamp = span.EndTimestamp;
            annotations = span.Annotations.ToDictionary(p => p.Key, p => p.Value);
        }

        public void SetAnnotation(string key, object value, bool allowOverwrite = true)
        {
            if (allowOverwrite || !annotations.ContainsKey(key))
                annotations[key] = value;
        }
    }
}