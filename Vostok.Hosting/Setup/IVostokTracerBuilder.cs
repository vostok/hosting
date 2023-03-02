using System;
using JetBrains.Annotations;
using Vostok.Tracing;
using Vostok.Tracing.Abstractions;
#if NET6_0_OR_GREATER
using Vostok.Tracing.Diagnostics;
#endif

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokTracerBuilder
    {
#if NET6_0_OR_GREATER
        bool UseActivitySourceTracer { get; set; }
        
        IVostokTracerBuilder CustomizeActivitySourceTracerSettings([NotNull] Action<ActivitySourceTracerSettings> settingsCustomization);
#endif

        IVostokTracerBuilder SetTracerProvider([NotNull] Func<TracerSettings, ITracer> tracerProvider);

        IVostokTracerBuilder SetupHerculesSpanSender([NotNull] Action<IVostokHerculesSpanSenderBuilder> herculesSpanSenderSetup);

        IVostokTracerBuilder AddSpanSender([NotNull] ISpanSender spanSender);

        IVostokTracerBuilder CustomizeSettings([NotNull] Action<TracerSettings> settingsCustomization);

        IVostokTracerBuilder CustomizeTracer([NotNull] Func<ITracer, ITracer> tracerCustomization);
    }
}