using System.Collections.Generic;
using System.Linq;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.Metrics;
using Vostok.Metrics.Senders;

namespace Vostok.Hosting.Components.Metrics
{
    internal class MetricsBuilder : IMetricsBuilder, IBuilder<IVostokApplicationMetrics>
    {
        private readonly HerculesMetricEventSenderBuilder herculesMetricEventSenderBuilder;
        private readonly List<IBuilder<IMetricEventSender>> metricEventSenderBuilders;

        public MetricsBuilder()
        {
            herculesMetricEventSenderBuilder = new HerculesMetricEventSenderBuilder();
            metricEventSenderBuilders = new List<IBuilder<IMetricEventSender>> {herculesMetricEventSenderBuilder};
        }

        public IMetricsBuilder SetupHerculesMetricEventSender(EnvironmentSetup<IHerculesMetricEventSenderBuilder> herculesMetricEventSenderSetup)
        {
            herculesMetricEventSenderSetup(herculesMetricEventSenderBuilder);
            return this;
        }

        public IMetricsBuilder AddMetricEventSender(IMetricEventSender metricEventSender)
        {
            metricEventSenderBuilders.Add(new CustomMetricEventSenderBuilder(metricEventSender));
            return this;
        }

        public IVostokApplicationMetrics Build(BuildContext context)
        {
            var sender = BuildCompositeMetricEventSender(context);

            if (sender == null)
                return new VostokApplicationMetrics(new DevNullMetricContext(), context.ApplicationIdentity);

            var settings = new MetricContextConfig(sender)
            {
                ErrorCallback = e => context.Log.Error(e, "Failed to send metrics.")
            };

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