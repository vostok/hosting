using System;
using JetBrains.Annotations;
using Vostok.Tracing;
using Vostok.Tracing.Abstractions;
#if NET6_0_OR_GREATER
using System.Diagnostics;
using Vostok.Tracing.Diagnostics;
#endif

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokTracerBuilder
    {
#if NET6_0_OR_GREATER
        /// <summary>
        /// <para>If set to <c>true</c> creates <see cref="UseActivitySourceTracer"/> instead of <see cref="Tracer"/>.</para>
        /// <para>This allows using <see cref="Activity"/> together with <see cref="ITracer"/>.</para>
        /// <para>Do not forget to setup of <c>OpenTelemetry.Trace.TracerProvider</c> and exporter yourself.</para>
         /// </summary>
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