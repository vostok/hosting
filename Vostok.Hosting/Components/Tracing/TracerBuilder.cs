using System;
using System.Collections.Generic;
using System.Linq;
using Vostok.Hosting.Helpers;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.Tracing;
using Vostok.Tracing.Abstractions;
using Vostok.ZooKeeper.Client;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Tracing
{
    internal class TracerBuilder : IVostokTracerBuilder, IBuilder<ITracer>
    {
        private Func<TracerSettings, ILog, ITracer> tracerProvider;
        private readonly HerculesSpanSenderBuilder herculesSpanSenderBuilder;
        private readonly List<IBuilder<ISpanSender>> spanSenderBuilders;
        private readonly Customization<TracerSettings> settingsCustomization;

        public TracerBuilder()
        {
            tracerProvider = (settings, log) => new Tracer(settings);
            herculesSpanSenderBuilder = new HerculesSpanSenderBuilder();

            spanSenderBuilders = new List<IBuilder<ISpanSender>> {herculesSpanSenderBuilder};

            settingsCustomization = new Customization<TracerSettings>();
        }

        public IVostokTracerBuilder SetTracerProvider(Func<TracerSettings, ILog, ITracer> tracerProvider)
        {
            this.tracerProvider = tracerProvider;
            return this;
        }

        public IVostokTracerBuilder SetupHerculesSpanSender(Action<IVostokHerculesSpanSenderBuilder> herculesSpanSenderSetup)
        {
            herculesSpanSenderSetup(herculesSpanSenderBuilder);
            return this;
        }

        public IVostokTracerBuilder AddSpanSender(ISpanSender spanSender)
        {
            spanSenderBuilders.Add(new CustomSpanSenderBuilder(spanSender));
            return this;
        }

        public IVostokTracerBuilder CustomizeSettings(Action<TracerSettings> settingsCustomization)
        {
            this.settingsCustomization.AddCustomization(settingsCustomization);
            return this;
        }

        public ITracer Build(BuildContext context)
        {
            var spanSender = BuildCompositeSpanSender(context);

            var settings = new TracerSettings(spanSender)
            {
                Application = context.ApplicationIdentity?.Application,
                Environment = context.ApplicationIdentity?.Environment
            };

            settingsCustomization.Customize(settings);

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