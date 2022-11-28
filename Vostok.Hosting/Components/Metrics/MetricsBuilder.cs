using System;
using System.Collections.Generic;
using System.Linq;
using Vostok.Commons.Helpers;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Helpers;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.Metrics;
using Vostok.Metrics.Senders;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Metrics
{
    internal class MetricsBuilder : IVostokMetricsBuilder, IBuilder<IVostokApplicationMetrics>
    {
        private readonly HerculesMetricEventSenderBuilder herculesMetricEventSenderBuilder;
        private readonly List<IBuilder<IMetricEventSender>> metricEventSenderBuilders;
        private readonly Customization<MetricContextConfig> settingsCustomization;
        private readonly Customization<IAnnotationEventSender> annotationEventSenderCustomization;
        private volatile bool addLoggingSender;

        public MetricsBuilder()
        {
            herculesMetricEventSenderBuilder = new HerculesMetricEventSenderBuilder();
            metricEventSenderBuilders = new List<IBuilder<IMetricEventSender>> {herculesMetricEventSenderBuilder};
            settingsCustomization = new Customization<MetricContextConfig>();
            annotationEventSenderCustomization = new Customization<IAnnotationEventSender>();
        }

        public IVostokMetricsBuilder SetupHerculesMetricEventSender(Action<IVostokHerculesMetricEventSenderBuilder> herculesMetricEventSenderSetup)
        {
            herculesMetricEventSenderBuilder.AutoEnable();
            herculesMetricEventSenderSetup(herculesMetricEventSenderBuilder ?? throw new ArgumentNullException(nameof(herculesMetricEventSenderSetup)));
            return this;
        }

        public IVostokMetricsBuilder SetupLoggingMetricEventSender()
        {
            addLoggingSender = true;
            return this;
        }

        public IVostokMetricsBuilder AddMetricEventSender(IMetricEventSender metricEventSender)
        {
            metricEventSenderBuilders.Add(
                new CustomBuilder<IMetricEventSender>(
                    metricEventSender ?? throw new ArgumentNullException(nameof(metricEventSender))));
            return this;
        }

        public IVostokMetricsBuilder CustomizeSettings(Action<MetricContextConfig> settingsCustomization)
        {
            this.settingsCustomization.AddCustomization(settingsCustomization ?? throw new ArgumentNullException(nameof(settingsCustomization)));
            return this;
        }

        public IVostokMetricsBuilder CustomizeAnnotationEventSender(Func<IAnnotationEventSender, IAnnotationEventSender> senderCustomization)
        {
            annotationEventSenderCustomization.AddCustomization(senderCustomization);
            return this;
        }

        public IVostokApplicationMetrics Build(BuildContext context)
        {
            var metricSender = BuildCompositeMetricEventSender(context, out var senders);
            if (metricSender == null)
                return new VostokApplicationMetrics(new DevNullMetricContext(), context.ApplicationIdentity);

            var annotationSender = BuildCompositeAnnotationEventSender(senders);

            var settings = new MetricContextConfig(metricSender)
            {
                ErrorCallback = e => context.Log.ForContext<MetricContext>().Error(e, "Failed to send metrics."),

                AnnotationSender = annotationSender
            };

            settingsCustomization.Customize(settings);

            var root = new MetricContext(settings);

            return new VostokApplicationMetrics(root, context.ApplicationIdentity);
        }

        private IMetricEventSender BuildCompositeMetricEventSender(BuildContext context, out List<IMetricEventSender> senders)
        {
            senders = metricEventSenderBuilders
                .Select(b => b.Build(context))
                .Where(s => s != null)
                .ToList();

            if (addLoggingSender)
                senders.Add(new LoggingMetricEventSender(context.Log));

            return senders.Count switch
            {
                0 => null,
                1 => senders.Single(),
                _ => new CompositeMetricEventSender(senders.ToArray())
            };
        }

        private IAnnotationEventSender BuildCompositeAnnotationEventSender(IEnumerable<IMetricEventSender> metricSenders)
        {
            var senders = metricSenders.OfType<IAnnotationEventSender>().ToArray();

            var annotationSender = senders.Length switch
            {
                0 => null,
                1 => senders.Single(),
                _ => new CompositeAnnotationEventSender(senders)
            };

            if (annotationSender != null)
                annotationSender = annotationEventSenderCustomization.Customize(annotationSender);

            return annotationSender;
        }
    }
}