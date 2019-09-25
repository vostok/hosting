using System;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Abstractions.Wrappers;
using Vostok.Tracing;
using Vostok.Tracing.Abstractions;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Tracing
{
    internal class TracerBuilder : ITracerBuilder, IBuilder<ITracer>
    {
        private Func<TracerSettings, ILog, ITracer> tracerProvider;
        private HerculesSpanSenderBuilder herculesSpanSenderBuilder;

        public TracerBuilder()
        {
            tracerProvider = (settings, log) => new Tracer(settings);
            herculesSpanSenderBuilder = new HerculesSpanSenderBuilder();
        }

        public ITracerBuilder SetTracerProvider(Func<TracerSettings, ILog, ITracer> tracerProvider)
        {
            this.tracerProvider = tracerProvider;
            return this;
        }

        public ITracerBuilder SetupHerculesSpanSender(EnvironmentSetup<IHerculesSpanSenderBuilder> herculesSpanSenderSetup)
        {
            herculesSpanSenderSetup(herculesSpanSenderBuilder);
            return this;
        }

        public ITracer Build(BuildContext context)
        {
            var spanSender = herculesSpanSenderBuilder.Build(context);

            var settings = new TracerSettings(spanSender)
            {
                Application = context.ApplicationIdentity.Application,
                Environment = context.ApplicationIdentity.Environment
            };

            var tracer = tracerProvider(settings, context.Log);

            return tracer;
        }
    }
}