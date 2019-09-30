using Vostok.Clusterclient.Core;
using Vostok.ClusterConfig.Client.Abstractions;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Components.Log;
using Vostok.Hosting.Components.Tracing;
using Vostok.Logging.Abstractions;
using Vostok.ServiceDiscovery.Abstractions;
using Vostok.Tracing.Abstractions;

namespace Vostok.Hosting.Components
{
    internal class BuildContext
    {
        public IVostokApplicationIdentity ApplicationIdentity { get; set; }
        public IServiceLocator ServiceLocator { get; set; }
        public IClusterConfigClient ClusterConfigClient { get; set; }
        public IHerculesSink HerculesSink { get; set; }
        public ClusterClientSetup ClusterClientSetup { get; set; }

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

        private readonly SubstitutableLog substitutableLog;

        private readonly SubstitutableTracer substitutableTracer;
        
        public BuildContext()
        {
            substitutableLog = new SubstitutableLog();
            substitutableTracer = new SubstitutableTracer();
        }
    }
}