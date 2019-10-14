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

        public void SubstituteWith(ITracer newTracer, TracerSettings tracerSettings)
        {
            baseTracer = newTracer;
            baseSender.SubstituteWith(tracerSettings);
        }

        public ISpanBuilder BeginSpan() =>
            baseTracer.BeginSpan();

        public TraceContext CurrentContext
        {
            get => baseTracer.CurrentContext;
            set => baseTracer.CurrentContext = value;
        }
    }
}