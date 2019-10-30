using System;
using System.Collections.Generic;
using System.Linq;
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

        public MetricsBuilder()
        {
            herculesMetricEventSenderBuilder = new HerculesMetricEventSenderBuilder();
            metricEventSenderBuilders = new List<IBuilder<IMetricEventSender>> {herculesMetricEventSenderBuilder};
            settingsCustomization = new Customization<MetricContextConfig>();
        }

        public IVostokMetricsBuilder SetupHerculesMetricEventSender()
        {
            herculesMetricEventSenderBuilder.Enable();
            return this;
        }

        public IVostokMetricsBuilder SetupHerculesMetricEventSender(Action<IVostokHerculesMetricEventSenderBuilder> herculesMetricEventSenderSetup)
        {
            herculesMetricEventSenderBuilder.Enable();
            herculesMetricEventSenderSetup(herculesMetricEventSenderBuilder ?? throw new ArgumentNullException(nameof(herculesMetricEventSenderSetup)));
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

        public IVostokApplicationMetrics Build(BuildContext context)
        {
            var sender = BuildCompositeMetricEventSender(context);
            if (sender == null)
                return new VostokApplicationMetrics(new DevNullMetricContext(), context.ApplicationIdentity);

            var settings = new MetricContextConfig(sender)
            {
                ErrorCallback = e => context.Log.ForContext<MetricContext>().Error(e, "Failed to send metrics.")
            };

            settingsCustomization.Customize(settings);

            var root = new MetricContext(settings);

            return new VostokApplicationMetrics(root, context.ApplicationIdentity);
        }

        private IMetricEventSender BuildCompositeMetricEventSender(BuildContext context)
        {
            var metricEventSenders = metricEventSenderBuilders
                .Select(b => b.Build(context))
                .Where(s => s != null)
                .ToArray();

            switch (metricEventSenders.Length)
            {
                case 0:
                    return null;
                case 1:
                    return metricEventSenders.Single();
                default:
                    return new CompositeMetricEventSender(metricEventSenders);
            }
        }
    }
}