using System;
using JetBrains.Annotations;
using Vostok.Logging.Abstractions;
using Vostok.Tracing;
using Vostok.Tracing.Abstractions;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface ITracerBuilder
    {
        ITracerBuilder SetTracerProvider(Func<TracerSettings, ILog, ITracer> tracerProvider);

        ITracerBuilder SetupHerculesSpanSender([NotNull] Action<IHerculesSpanSenderBuilder> herculesSpanSenderSetup);

        ITracerBuilder AddSpanSender([NotNull] ISpanSender spanSender);
    }
}