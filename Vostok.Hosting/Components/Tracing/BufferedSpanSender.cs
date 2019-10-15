using Vostok.Commons.Collections;
using Vostok.Tracing;
using Vostok.Tracing.Abstractions;

namespace Vostok.Hosting.Components.Tracing
{
    internal class BufferedSpanSender : ISpanSender
    {
        private const int Capacity = 1_000;
        private readonly ConcurrentBoundedQueue<ISpan> queue;

        public BufferedSpanSender()
        {
            queue = new ConcurrentBoundedQueue<ISpan>(Capacity);
        }

        public void SendBufferedSpans(TracerSettings tracerSettings)
        {
            var buffer = new ISpan[Capacity];
            var count = queue.Drain(buffer, 0, Capacity);
            for (var i = 0; i < count; i++)
            {
                var span = new Span(buffer[i]);

                if (tracerSettings.Host != null)
                    span.SetAnnotation(WellKnownAnnotations.Common.Host, tracerSettings.Host);
                if (tracerSettings.Application != null)
                    span.SetAnnotation(WellKnownAnnotations.Common.Application, tracerSettings.Application);
                if (tracerSettings.Environment != null)
                    span.SetAnnotation(WellKnownAnnotations.Common.Environment, tracerSettings.Environment);

                tracerSettings.Sender.Send(span);
            }
        }

        public void Send(ISpan span)
        {
            queue.TryAdd(span);
        }
    }
}