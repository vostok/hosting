using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Vostok.Commons.Time;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Abstractions;
using Vostok.Metrics;
using Vostok.ServiceDiscovery;
using Vostok.ServiceDiscovery.Abstractions;

namespace Vostok.Hosting.Components.Shutdown
{
    internal static class ShutdownFactory
    {
        public static (HostingShutdown hosting, ApplicationShutdown application) Create(
            IServiceBeacon serviceBeacon,
            IServiceLocator serviceLocator,
            IVostokApplicationIdentity identity,
            IMetricContext instanceMetrics,
            ILog log,
            int? port,
            IReadOnlyList<CancellationToken> tokens,
            TimeSpan totalTimeout,
            TimeSpan beaconTimeout,
            bool beaconWaitEnabled,
            bool sendAnnotation)
        {
            var hasRealBeacon = serviceBeacon is ServiceBeacon;
            var hasRealLocator = serviceLocator is ServiceLocator;

            // (iloktionov): No point in waiting for beacon deregistration for apps without external port or when SD is disabled.
            beaconWaitEnabled &= port.HasValue && hasRealBeacon && hasRealLocator;

            // (iloktionov): No point in reducing app shutdown timeout right from the start when SD is disabled.
            beaconTimeout = hasRealBeacon ? TimeSpanArithmetics.Min(beaconTimeout, totalTimeout.Divide(3)) : TimeSpan.Zero;

            // (iloktionov): Artificially reduce initial app shutdown timeout by beacon shutdown timeout so that it's value doesn't drop abruptly on shutdown.
            var applicationShutdown = new ApplicationShutdown(log, totalTimeout - beaconTimeout);

            var hostingToken = tokens.Any() ? CancellationTokenSource.CreateLinkedTokenSource(tokens.ToArray()).Token : default;

            var hostingShutdown = new HostingShutdown(
                applicationShutdown, 
                serviceBeacon, 
                serviceLocator,
                identity,
                instanceMetrics,
                log,
                hostingToken,
                totalTimeout,
                beaconTimeout, 
                beaconWaitEnabled,
                sendAnnotation);

            return (hostingShutdown, applicationShutdown);
        }
    }
}
