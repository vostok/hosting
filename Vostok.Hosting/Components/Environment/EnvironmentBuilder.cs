using System;
using System.Threading;
using Vostok.Context;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hosting.Components.ApplicationIdentity;
using Vostok.Hosting.Components.ClusterClient;
using Vostok.Hosting.Components.Configuration;
using Vostok.Hosting.Components.Hercules;
using Vostok.Hosting.Components.Log;
using Vostok.Hosting.Components.Metrics;
using Vostok.Hosting.Components.ServiceDiscovery;
using Vostok.Hosting.Components.Tracing;
using Vostok.Hosting.Components.ZooKeeper;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.Tracing.Abstractions;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Environment
{
    internal class EnvironmentBuilder : IEnvironmentBuilder
    {
        private readonly ApplicationIdentityBuilder applicationIdentityBuilder;
        private readonly CompositeLogBuilder compositeLogBuilder;
        private readonly HerculesSinkBuilder herculesSinkBuilder;
        private readonly ClusterConfigClientBuilder clusterConfigClientBuilder;
        private readonly TracerBuilder tracerBuilder;
        private readonly ClusterClientSetupBuilder clusterClientSetupBuilder;
        private readonly MetricsBuilder metricsBuilder;
        private readonly ZooKeeperClientBuilder zooKeeperClientBuilder;
        private readonly ServiceBeaconBuilder serviceBeaconBuilder;
        private readonly ServiceLocatorBuilder serviceLocatorBuilder;

        public EnvironmentBuilder()
        {
            applicationIdentityBuilder = new ApplicationIdentityBuilder();
            compositeLogBuilder = new CompositeLogBuilder();
            herculesSinkBuilder = new HerculesSinkBuilder();
            clusterConfigClientBuilder = new ClusterConfigClientBuilder();
            tracerBuilder = new TracerBuilder();
            clusterClientSetupBuilder = new ClusterClientSetupBuilder();
            metricsBuilder = new MetricsBuilder();
            zooKeeperClientBuilder = new ZooKeeperClientBuilder();
            serviceBeaconBuilder = new ServiceBeaconBuilder();
            serviceLocatorBuilder = new ServiceLocatorBuilder();
        }

        public VostokHostingEnvironment Build(CancellationToken shutdownToken)
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var context = new BuildContext();

            LogProvider.Configure(context.Log, true);
            TracerProvider.Configure(context.Tracer, true);
            
            context.ApplicationIdentity = applicationIdentityBuilder.Build(context);
            Substitute(context);

            context.ClusterConfigClient = clusterConfigClientBuilder.Build(context);
            Substitute(context);
            
            context.ApplicationIdentity = applicationIdentityBuilder.Build(context);
            Substitute(context);

            context.ZooKeeperClient = zooKeeperClientBuilder.Build(context);

            context.ServiceLocator = serviceLocatorBuilder.Build(context);

            context.HerculesSink = herculesSinkBuilder.Build(context);
            HerculesSinkProvider.Configure(context.HerculesSink, true);
            Substitute(context);

            context.Metrics = metricsBuilder.Build(context);

            FlowingContext.Configuration.ErrorCallback = (errorMessage, error) => context.Log.Error(error, errorMessage);

            return new VostokHostingEnvironment(
                shutdownToken,
                context.ApplicationIdentity,
                context.Metrics,
                context.Log,
                context.Tracer,
                context.HerculesSink,
                null,
                null,
                serviceBeaconBuilder.Build(context),
                context.ServiceLocator,
                FlowingContext.Globals,
                FlowingContext.Properties,
                FlowingContext.Configuration,
                clusterClientSetupBuilder.Build(context),
                null,
                DisposeEnvironment);
        }

        public IEnvironmentBuilder SetupLog(Action<ICompositeLogBuilder> compositeLogSetup)
        {
            compositeLogSetup(compositeLogBuilder);
            return this;
        }

        public IEnvironmentBuilder SetupClusterConfigClient(Action<IClusterConfigClientBuilder> clusterConfigClientSetup)
        {
            clusterConfigClientSetup(clusterConfigClientBuilder);
            return this;
        }

        public IEnvironmentBuilder SetupApplicationIdentity(Action<IApplicationIdentityBuilder> applicationIdentitySetup)
        {
            applicationIdentitySetup(applicationIdentityBuilder);
            return this;
        }

        public IEnvironmentBuilder SetupTracer(Action<ITracerBuilder> tracerSetup)
        {
            tracerSetup(tracerBuilder);
            return this;
        }

        public IEnvironmentBuilder SetupMetrics(Action<IMetricsBuilder> metricsSetup)
        {
            metricsSetup(metricsBuilder);
            return this;
        }

        public IEnvironmentBuilder SetupClusterClientSetup(Action<IClusterClientSetupBuilder> clusterClientSetup)
        {
            clusterClientSetup(clusterClientSetupBuilder);
            return this;
        }

        public IEnvironmentBuilder SetupZooKeeperClient(Action<IZooKeeperClientBuilder> zooKeeperClientSetup)
        {
            zooKeeperClientSetup(zooKeeperClientBuilder);
            return this;
        }

        public IEnvironmentBuilder SetupHerculesSink(Action<IHerculesSinkBuilder> sinkSetup)
        {
            sinkSetup(herculesSinkBuilder);
            return this;
        }

        public IEnvironmentBuilder SetupServiceBeacon(Action<IServiceBeaconBuilder> serviceBeaconSetup)
        {
            serviceBeaconSetup(serviceBeaconBuilder);
            return this;
        }

        public IEnvironmentBuilder SetupServiceLocator(Action<IServiceLocatorBuilder> serviceLocatorSetup)
        {
            serviceLocatorSetup(serviceLocatorBuilder);
            return this;
        }

        private void DisposeEnvironment()
        {
        }
        
        private void Substitute(BuildContext context)
        {
            // Note(kungurtsev): requires hercules, not disposable and lightweight.
            // Note(kungurtsev): get returns same instance (with substitutable base), so can be executed in any order.
            context.Log = compositeLogBuilder.Build(context);
            context.Tracer = tracerBuilder.Build(context);
        }
    }
}