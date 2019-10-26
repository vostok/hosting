using System;
using System.Threading;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core;
using Vostok.ClusterConfig.Client.Abstractions;
using Vostok.Configuration.Abstractions;
using Vostok.Context;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Abstractions;
using Vostok.ServiceDiscovery.Abstractions;
using Vostok.Tracing.Abstractions;

namespace Vostok.Hosting.Models
{
    internal class VostokHostingEnvironment : IVostokHostingEnvironment, IDisposable
    {
        private readonly Func<IVostokApplicationReplicationInfo> applicationReplicationInfoProvider;
        private readonly Action dispose;

        internal VostokHostingEnvironment(
            CancellationToken shutdownToken,
            [NotNull] IVostokApplicationIdentity applicationIdentity,
            [NotNull] IVostokApplicationLimits applicationLimits,
            [NotNull] Func<IVostokApplicationReplicationInfo> applicationReplicationInfoProvider,
            [NotNull] IVostokApplicationMetrics metrics,
            [NotNull] ILog log,
            [NotNull] ITracer tracer,
            [NotNull] IHerculesSink herculesSink,
            [NotNull] IConfigurationSource configurationSource,
            [NotNull] IConfigurationProvider configurationProvider,
            [NotNull] IClusterConfigClient clusterConfigClient,
            [NotNull] IServiceBeacon serviceBeacon,
            [NotNull] IServiceLocator serviceLocator,
            [NotNull] IContextGlobals contextGlobals,
            [NotNull] IContextProperties contextProperties,
            [NotNull] IContextConfiguration contextConfiguration,
            [NotNull] ClusterClientSetup clusterClientSetup,
            [NotNull] IVostokHostExtensions hostExtensions,
            [NotNull] Action dispose)
        {
            this.applicationReplicationInfoProvider = applicationReplicationInfoProvider ?? throw new ArgumentNullException(nameof(applicationReplicationInfoProvider));
            this.dispose = dispose ?? throw new ArgumentNullException(nameof(dispose));

            ShutdownToken = shutdownToken;
            ApplicationIdentity = applicationIdentity ?? throw new ArgumentNullException(nameof(applicationIdentity));
            ApplicationLimits = applicationLimits ?? throw new ArgumentNullException(nameof(applicationLimits));
            Metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
            Log = log ?? throw new ArgumentNullException(nameof(log));
            Tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
            HerculesSink = herculesSink ?? throw new ArgumentNullException(nameof(herculesSink));
            ConfigurationSource = configurationSource ?? throw new ArgumentNullException(nameof(configurationSource));
            ConfigurationProvider = configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));
            ClusterConfigClient = clusterConfigClient ?? throw new ArgumentNullException(nameof(clusterConfigClient));
            ServiceBeacon = serviceBeacon ?? throw new ArgumentNullException(nameof(serviceBeacon));
            ServiceLocator = serviceLocator ?? throw new ArgumentNullException(nameof(serviceLocator));
            ContextGlobals = contextGlobals ?? throw new ArgumentNullException(nameof(contextGlobals));
            ContextProperties = contextProperties ?? throw new ArgumentNullException(nameof(contextProperties));
            ContextConfiguration = contextConfiguration ?? throw new ArgumentNullException(nameof(contextConfiguration));
            ClusterClientSetup = clusterClientSetup ?? throw new ArgumentNullException(nameof(clusterClientSetup));
            HostExtensions = hostExtensions ?? throw new ArgumentNullException(nameof(hostExtensions));
        }

        public CancellationToken ShutdownToken { get; }
        public IVostokApplicationIdentity ApplicationIdentity { get; }
        public IVostokApplicationLimits ApplicationLimits { get; }
        public IVostokApplicationReplicationInfo ApplicationReplicationInfo => applicationReplicationInfoProvider();
        public IVostokApplicationMetrics Metrics { get; }
        public ILog Log { get; }
        public ITracer Tracer { get; }
        public IHerculesSink HerculesSink { get; }
        public IConfigurationSource ConfigurationSource { get; }
        public IConfigurationProvider ConfigurationProvider { get; }
        public IClusterConfigClient ClusterConfigClient { get; }
        public IServiceBeacon ServiceBeacon { get; }
        public IServiceLocator ServiceLocator { get; }
        public IContextGlobals ContextGlobals { get; }
        public IContextProperties ContextProperties { get; }
        public IContextConfiguration ContextConfiguration { get; }
        public ClusterClientSetup ClusterClientSetup { get; }
        public IVostokHostExtensions HostExtensions { get; }

        public void Dispose()
            => dispose();
    }
}