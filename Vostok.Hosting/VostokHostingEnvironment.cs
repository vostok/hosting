using System;
using System.Threading;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core;
using Vostok.Configuration.Abstractions;
using Vostok.Context;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Abstractions;
using Vostok.ServiceDiscovery.Abstractions;
using Vostok.Tracing.Abstractions;

// ReSharper disable NotNullMemberIsNotInitialized

namespace Vostok.Hosting
{
    // CR(iloktionov): Probably shouldn't be public.
    [PublicAPI]
    public class VostokHostingEnvironment : IVostokHostingEnvironment, IDisposable
    {
        private readonly Action dispose;

        internal VostokHostingEnvironment(
            CancellationToken shutdownToken,
            IVostokApplicationIdentity applicationIdentity,
            IVostokApplicationMetrics metrics,
            ILog log,
            ITracer tracer,
            IHerculesSink herculesSink,
            IConfigurationSource configurationSource,
            IConfigurationProvider configurationProvider,
            IServiceBeacon serviceBeacon,
            IServiceLocator serviceLocator,
            IContextGlobals contextGlobals,
            IContextProperties contextProperties,
            IContextConfiguration contextConfiguration,
            ClusterClientSetup clusterClientSetup,
            IVostokHostExtensions hostExtensions,
            Action dispose)
        {
            this.dispose = dispose;
            ShutdownToken = shutdownToken;
            ApplicationIdentity = applicationIdentity;
            Metrics = metrics;
            Log = log;
            Tracer = tracer;
            HerculesSink = herculesSink;
            ConfigurationSource = configurationSource;
            ConfigurationProvider = configurationProvider;
            ServiceBeacon = serviceBeacon;
            ServiceLocator = serviceLocator;
            ContextGlobals = contextGlobals;
            ContextProperties = contextProperties;
            ContextConfiguration = contextConfiguration;
            ClusterClientSetup = clusterClientSetup;
            HostExtensions = hostExtensions;
        }

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

        public void Dispose()
        {
            dispose();
        }
    }
}