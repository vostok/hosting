using Vostok.Tracing;
using Vostok.Tracing.Abstractions;

namespace Vostok.Hosting.Components.Tracing
{
    internal class SubstitutableSpanSender : ISpanSender
    {
        private volatile ISpanSender baseSender = new BufferedSpanSender();
        
        public void SubstituteWith(TracerSettings tracerSettings)
        {
            var oldSender = baseSender;

            baseSender = tracerSettings.Sender;

            if (oldSender is BufferedSpanSender bufferedSpanSender)
            {
                bufferedSpanSender.SendBufferedSpans(tracerSettings);
            }
        }

        public void Send(ISpan span) =>
            baseSender.Send(span);
    }
}