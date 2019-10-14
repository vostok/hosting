using Vostok.Commons.Collections;
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
        
        public void SendBufferedSpans(ISpanSender sender)
        {
            var buffer = new ISpan[Capacity];
            var count = queue.Drain(buffer, 0, Capacity);
            for (var i = 0; i < count; i++)
                sender.Send(buffer[i]);
        }

        public void Send(ISpan span)
        {
            queue.TryAdd(span);
        }
    }
}