using System;
using System.Collections.Generic;
using System.Linq;
using Vostok.Commons.Helpers;
using Vostok.Hosting.Helpers;
using Vostok.Hosting.Setup;
using Vostok.ServiceDiscovery;
using Vostok.Tracing;
using Vostok.Tracing.Abstractions;
#if NET6_0_OR_GREATER
using Vostok.Tracing.Diagnostics;
#endif

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Tracing
{
    internal class TracerBuilder : IVostokTracerBuilder, IBuilder<(ITracer, TracerSettings)>
    {
        private readonly HerculesSpanSenderBuilder herculesSpanSenderBuilder;
        private readonly List<IBuilder<ISpanSender>> spanSenderBuilders;
        private readonly Customization<TracerSettings> settingsCustomization;
#if NET6_0_OR_GREATER
        private readonly Customization<ActivitySourceTracerSettings> activitySourceTracerSettingsCustomization;
#endif
        private readonly Customization<ITracer> tracerCustomization;
        private volatile Func<TracerSettings, ITracer> tracerProvider;

        public TracerBuilder()
        {
            tracerProvider = settings => new Tracer(settings);
            herculesSpanSenderBuilder = new HerculesSpanSenderBuilder();
            spanSenderBuilders = new List<IBuilder<ISpanSender>> {herculesSpanSenderBuilder};
            settingsCustomization = new Customization<TracerSettings>();
#if NET6_0_OR_GREATER
            activitySourceTracerSettingsCustomization = new Customization<ActivitySourceTracerSettings>();
#endif
            tracerCustomization = new Customization<ITracer>();
        }

#if NET6_0_OR_GREATER
        public bool UseActivitySourceTracer { get; set; }
        
        public IVostokTracerBuilder CustomizeActivitySourceTracerSettings(Action<ActivitySourceTracerSettings> settingsCustomization)
        {
            activitySourceTracerSettingsCustomization.AddCustomization(settingsCustomization ?? throw new ArgumentNullException(nameof(settingsCustomization)));
            return this;
        }
#endif

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
#if NET6_0_OR_GREATER
            if (UseActivitySourceTracer)
            {
                var activitySourceTracerSettings = new ActivitySourceTracerSettings();
                activitySourceTracerSettingsCustomization.Customize(activitySourceTracerSettings);
                ITracer activitySourceTracer = new ActivitySourceTracer(activitySourceTracerSettings);
                activitySourceTracer = tracerCustomization.Customize(activitySourceTracer);
                return (activitySourceTracer, new TracerSettings(new DevNullSpanSender()));
            }
#endif
            
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