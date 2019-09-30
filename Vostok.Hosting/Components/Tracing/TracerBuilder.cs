using System;
using System.Collections.Generic;
using System.Linq;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.Tracing;
using Vostok.Tracing.Abstractions;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Tracing
{
    internal class TracerBuilder : ITracerBuilder, IBuilder<ITracer>
    {
        private Func<TracerSettings, ILog, ITracer> tracerProvider;
        private HerculesSpanSenderBuilder herculesSpanSenderBuilder;

        private readonly List<IBuilder<ISpanSender>> spanSenderBuilders;

        public TracerBuilder()
        {
            tracerProvider = (settings, log) => new Tracer(settings);
            herculesSpanSenderBuilder = new HerculesSpanSenderBuilder();

            spanSenderBuilders = new List<IBuilder<ISpanSender>> {herculesSpanSenderBuilder};
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

        public ITracerBuilder AddSpanSender(ISpanSender spanSender)
        {
            spanSenderBuilders.Add(new CustomSpanSenderBuilder(spanSender));
            return this;
        }

        public ITracer Build(BuildContext context)
        {
            var spanSender = BuildCompositeSpanSender(context);

            var settings = new TracerSettings(spanSender)
            {
                Application = context.ApplicationIdentity.Application,
                Environment = context.ApplicationIdentity.Environment
            };

            var tracer = tracerProvider(settings, context.Log);

            return tracer;
        }

        private ISpanSender BuildCompositeSpanSender(BuildContext context)
        {
            var spanSenders = spanSenderBuilders
                .Select(b => b.Build(context))
                .Where(s => s != null)
                .ToArray();

            switch (spanSenders.Length)
            {
                case 0:
                    return new DevNullSpanSender();
                case 1:
                    return spanSenders.Single();
                default:
                    return new CompositeSpanSender(spanSenders);
            }
        }
    }
}