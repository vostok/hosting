using Vostok.Tracing.Abstractions;

namespace Vostok.Hosting.Components.Tracing
{
    internal class SubstitutableSpanSender : ISpanSender
    {
        private volatile ISpanSender baseSender = new BufferedSpanSender();
        
        public void SubstituteWith(ISpanSender newSender)
        {
            var oldSender = baseSender;

            baseSender = newSender;

            if (oldSender is BufferedSpanSender bufferedSpanSender)
            {
                bufferedSpanSender.SendBufferedSpans(newSender);
            }
        }

        public void Send(ISpan span) =>
            baseSender.Send(span);
    }
}