using Vostok.Metrics;

namespace Vostok.Hosting.Components.Metrics
{
    internal class CustomMetricEventSenderBuilder : IBuilder<IMetricEventSender>
    {
        private readonly IMetricEventSender metricEventSender;

        public CustomMetricEventSenderBuilder(IMetricEventSender metricEventSender)
        {
            this.metricEventSender = metricEventSender;
        }

        public IMetricEventSender Build(BuildContext context) => metricEventSender;
    }
}