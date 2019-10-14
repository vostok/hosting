using System;
using Vostok.Commons.Collections;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Abstractions.Wrappers;

namespace Vostok.Hosting.Components.Log
{
    internal class BufferedLog : ILog
    {
        private const int Capacity = 1_000;
        private readonly ConcurrentBoundedQueue<LogEvent> queue;

        public BufferedLog()
        {
            queue = new ConcurrentBoundedQueue<LogEvent>(Capacity);
        }

        public void Log(LogEvent @event)
        {
            queue.TryAdd(@event);
        }

        public bool IsEnabledFor(LogLevel level) => true;

        public ILog ForContext(string context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return new SourceContextWrapper(this, context);
        }

        public void SendBufferedEvents(ILog log)
        {
            var buffer = new LogEvent[Capacity];
            var count = queue.Drain(buffer, 0, Capacity);
            for (var i = 0; i < count; i++)
                log.Log(buffer[i]);
        }
    }
}