using System.Threading;
using Vostok.Clusterclient.Core;
using Vostok.Configuration.Abstractions;
using Vostok.Context;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Abstractions;
using Vostok.ServiceDiscovery.Abstractions;
using Vostok.Tracing.Abstractions;
// ReSharper disable NotNullMemberIsNotInitialized

namespace Vostok.Hosting.Components.Environment
{
    internal class VostokHostingEnvironment : IVostokHostingEnvironment
    {
        public CancellationToken ShutdownToken { get; set; }
        public IVostokApplicationIdentity ApplicationIdentity { get; set; }
        public IVostokApplicationMetrics Metrics { get; set; }
        public ILog Log { get; set; }
        public ITracer Tracer { get; set; }
        public IHerculesSink HerculesSink { get; set; }
        public IConfigurationSource ConfigurationSource { get; set; }
        public IConfigurationProvider ConfigurationProvider { get; set; }
        public IServiceBeacon ServiceBeacon { get; set; }
        public IServiceLocator ServiceLocator { get; set; }
        public IContextGlobals ContextGlobals { get; set; }
        public IContextProperties ContextProperties { get; set; }
        public IContextConfiguration ContextConfiguration { get; set; }
        public ClusterClientSetup ClusterClientSetup { get; set; }
        public IVostokHostExtensions HostExtensions { get; set; }
    }
}