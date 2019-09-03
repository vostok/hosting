using System.Threading;
using Vostok.Clusterclient.Core;
using Vostok.Configuration.Abstractions;
using Vostok.Context;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Abstractions;
using Vostok.ServiceDiscovery.Abstractions;
using Vostok.Tracing.Abstractions;

namespace Vostok.Hosting
{
    internal class VostokHostingEnvironment : IVostokHostingEnvironment
    {
        public CancellationToken ShutdownToken { get; }
        public IVostokApplicationIdentity ApplicationIdentity { get; }
        public IVostokApplicationMetrics Metrics { get; }
        public ILog Log { get; }
        public ITracer Tracer { get; }
        public IHerculesSink HerculesSink { get; }
        public IConfigurationSource ConfigurationSource { get; }
        public IConfigurationProvider ConfigurationProvider { get; }
        public IServiceBeacon ServiceBeacon { get; }
        public IServiceLocator ServiceLocator { get; }
        public IContextGlobals ContextGlobals { get; }
        public IContextProperties ContextProperties { get; }
        public IContextConfiguration ContextConfiguration { get; }
        public ClusterClientSetup ClusterClientSetup { get; }
        public IVostokHostExtensions HostExtensions { get; }
    }
}