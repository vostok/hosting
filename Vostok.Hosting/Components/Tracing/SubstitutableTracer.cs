using Vostok.Tracing;
using Vostok.Tracing.Abstractions;

namespace Vostok.Hosting.Components.Tracing
{
    internal class SubstitutableTracer : ITracer
    {
        private volatile SubstitutableSpanSender baseSender;
        private volatile ITracer baseTracer;

        public SubstitutableTracer()
        {
            baseSender = new SubstitutableSpanSender();
            baseTracer = new Tracer(new TracerSettings(baseSender));
        }

        public TraceContext CurrentContext
        {
            get => baseTracer.CurrentContext;
            set => baseTracer.CurrentContext = value;
        }

        public void SubstituteWith(ITracer newTracer, TracerSettings tracerSettings)
        {
            baseTracer = newTracer;
            if (tracerSettings != null)
                baseSender.SubstituteWith(tracerSettings);
            else
                baseSender = null;
        }

        public ISpanBuilder BeginSpan() =>
            baseTracer.BeginSpan();
    }
}