using Vostok.ClusterConfig.Client.Abstractions;
using Vostok.Configuration.Abstractions;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Components.Log;
using Vostok.Hosting.Components.Tracing;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.ServiceDiscovery.Abstractions;
using Vostok.Tracing;
using Vostok.Tracing.Abstractions;
using Vostok.ZooKeeper.Client.Abstractions;

namespace Vostok.Hosting.Components
{
    internal class BuildContext
    {
        public IVostokApplicationIdentity ApplicationIdentity { get; set; }
        public IServiceLocator ServiceLocator { get; set; }
        public IClusterConfigClient ClusterConfigClient { get; set; }
        public IConfigurationSource ConfigurationSource { get; set; }
        public IConfigurationProvider ConfigurationProvider { get; set; }
        public IHerculesSink HerculesSink { get; set; }
        public IVostokApplicationMetrics Metrics { get; set; }
        public IZooKeeperClient ZooKeeperClient { get; set; }

        public IVostokConfigurationContext ConfigurationContext { get; set; }

        public ILog Log
        {
            get => substitutableLog;
            set => substitutableLog.SubstituteWith(value);
        }

        public ITracer Tracer
        {
            get => substitutableTracer;
            set => substitutableTracer.SubstituteWith(value);
        }

        public ISpanSender SpanSender
        {
            get => substitutableSpanSender;
            set => substitutableSpanSender.SubstituteWith(value);
        }

        private readonly SubstitutableLog substitutableLog;

        private readonly SubstitutableTracer substitutableTracer;

        private readonly SubstitutableSpanSender substitutableSpanSender;
        
        public BuildContext()
        {
            substitutableLog = new SubstitutableLog();
            substitutableSpanSender = new SubstitutableSpanSender();

            var tracer = new Tracer(new TracerSettings(SpanSender));
            substitutableTracer = new SubstitutableTracer(tracer);
        }
    }
}