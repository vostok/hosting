using System;
using JetBrains.Annotations;
using Vostok.Tracing;
using Vostok.Tracing.Abstractions;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokTracerBuilder
    {
        IVostokTracerBuilder SetTracerProvider(Func<TracerSettings, ITracer> tracerProvider);

        IVostokTracerBuilder SetupHerculesSpanSender([NotNull] Action<IVostokHerculesSpanSenderBuilder> herculesSpanSenderSetup);

        IVostokTracerBuilder AddSpanSender([NotNull] ISpanSender spanSender);

        IVostokTracerBuilder CustomizeSettings([NotNull] Action<TracerSettings> settingsCustomization);
    }
}