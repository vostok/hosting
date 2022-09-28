using System;
using System.Collections.Generic;
using System.Linq;
using Vostok.Commons.Helpers;
using Vostok.Hosting.Helpers;
using Vostok.Hosting.Setup;
using Vostok.ServiceDiscovery;
using Vostok.Tracing;
using Vostok.Tracing.Abstractions;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Tracing
{
    internal class TracerBuilder : IVostokTracerBuilder, IBuilder<(ITracer, TracerSettings)>
    {
        private readonly HerculesSpanSenderBuilder herculesSpanSenderBuilder;
        private readonly List<IBuilder<ISpanSender>> spanSenderBuilders;
        private readonly Customization<TracerSettings> settingsCustomization;
        private readonly Customization<ITracer> tracerCustomization;
        private volatile Func<TracerSettings, ITracer> tracerProvider;

        public TracerBuilder()
        {
            tracerProvider = settings => new Tracer(settings);
            herculesSpanSenderBuilder = new HerculesSpanSenderBuilder();
            spanSenderBuilders = new List<IBuilder<ISpanSender>> {herculesSpanSenderBuilder};
            settingsCustomization = new Customization<TracerSettings>();
            tracerCustomization = new Customization<ITracer>();
        }

        public IVostokTracerBuilder SetTracerProvider(Func<TracerSettings, ITracer> tracerProvider)
        {
            this.tracerProvider = tracerProvider ?? throw new ArgumentNullException(nameof(tracerProvider));
            return this;
        }

        public IVostokTracerBuilder SetupHerculesSpanSender(Action<IVostokHerculesSpanSenderBuilder> herculesSpanSenderSetup)
        {
            herculesSpanSenderSetup = herculesSpanSenderSetup ?? throw new ArgumentNullException(nameof(herculesSpanSenderSetup));

            herculesSpanSenderBuilder.AutoEnable();
            herculesSpanSenderSetup(herculesSpanSenderBuilder);
            return this;
        }

        public IVostokTracerBuilder AddSpanSender(ISpanSender spanSender)
        {
            spanSenderBuilders.Add(new CustomBuilder<ISpanSender>(spanSender ?? throw new ArgumentNullException(nameof(spanSender))));
            return this;
        }

        public IVostokTracerBuilder CustomizeSettings(Action<TracerSettings> settingsCustomization)
        {
            this.settingsCustomization.AddCustomization(settingsCustomization ?? throw new ArgumentNullException(nameof(settingsCustomization)));
            return this;
        }

        public IVostokTracerBuilder CustomizeTracer(Func<ITracer, ITracer> tracerCustomization)
        {
            this.tracerCustomization.AddCustomization(tracerCustomization ?? throw new ArgumentNullException(nameof(tracerCustomization)));
            return this;
        }

        public (ITracer, TracerSettings) Build(BuildContext context)
        {
            var spanSender = BuildCompositeSpanSender(context);

            var settings = new TracerSettings(spanSender)
            {
                Application = context.ApplicationIdentity.FormatServiceName(),
                Environment = context.ApplicationIdentity.Environment
            };

            if (context.ServiceBeacon is ServiceBeacon beacon)
            {
                settings.Application = beacon.ReplicaInfo.Application;
                settings.Environment = beacon.ReplicaInfo.Environment;
            }

            settingsCustomization.Customize(settings);

            var tracer = tracerCustomization.Customize(tracerProvider(settings));

            return (tracer, settings);
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