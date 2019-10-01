using System;
using Vostok.Clusterclient.Core;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hosting.Components.ApplicationIdentity;
using Vostok.Hosting.Components.ClusterClient;
using Vostok.Hosting.Components.Configuration;
using Vostok.Hosting.Components.Hercules;
using Vostok.Hosting.Components.Log;
using Vostok.Hosting.Components.Metrics;
using Vostok.Hosting.Components.ServiceBeacon;
using Vostok.Hosting.Components.ServiceDiscovery;
using Vostok.Hosting.Components.Tracing;
using Vostok.Hosting.Components.ZooKeeper;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.Tracing.Abstractions;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Environment
{
    internal class EnvironmentBuilder : IEnvironmentBuilder, IDisposable
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
        }

        public VostokHostingEnvironment Build()
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



            context.HerculesSink = herculesSinkBuilder.Build(context);
            HerculesSinkProvider.Configure(context.HerculesSink, true);
            Substitute(context);

            context.Metrics = metricsBuilder.Build(context);

            return new VostokHostingEnvironment
            {
                Log = context.Log,
                Tracer = context.Tracer,
                ApplicationIdentity = context.ApplicationIdentity,
                HerculesSink = context.HerculesSink,
                Metrics = context.Metrics,

                ServiceBeacon = serviceBeaconBuilder.Build(context),
                ClusterClientSetup = clusterClientSetupBuilder.Build(context)
            };
        }

        public IEnvironmentBuilder SetupLog(EnvironmentSetup<ICompositeLogBuilder> compositeLogSetup)
        {
            compositeLogSetup(compositeLogBuilder);
            return this;
        }

        public IEnvironmentBuilder SetupClusterConfigClient(EnvironmentSetup<IClusterConfigClientBuilder> clusterConfigClientSetup)
        {
            clusterConfigClientSetup(clusterConfigClientBuilder);
            return this;
        }

        public IEnvironmentBuilder SetupApplicationIdentity(EnvironmentSetup<IApplicationIdentityBuilder> applicationIdentitySetup)
        {
            applicationIdentitySetup(applicationIdentityBuilder);
            return this;
        }

        public IEnvironmentBuilder SetupTracer(EnvironmentSetup<ITracerBuilder> tracerSetup)
        {
            tracerSetup(tracerBuilder);
            return this;
        }

        public IEnvironmentBuilder SetupMetrics(EnvironmentSetup<IMetricsBuilder> metricsSetup)
        {
            metricsSetup(metricsBuilder);
            return this;
        }

        public IEnvironmentBuilder SetupClusterClientSetup(ClusterClientSetup clusterClientSetup)
        {
            clusterClientSetupBuilder.Setup(clusterClientSetup);
            return this;
        }

        public IEnvironmentBuilder SetupZooKeeperClient(EnvironmentSetup<IZooKeeperClientBuilder> zooKeeperClientSetup)
        {
            zooKeeperClientSetup(zooKeeperClientBuilder);
            return this;
        }

        public IEnvironmentBuilder SetupHerculesSink(EnvironmentSetup<IHerculesSinkBuilder> sinkSetup)
        {
            sinkSetup(herculesSinkBuilder);
            return this;
        }

        public IEnvironmentBuilder SetupServiceBeacon(EnvironmentSetup<IServiceBeaconBuilder> serviceBeaconSetup)
        {
            serviceBeaconSetup(serviceBeaconBuilder);
            return this;
        }

        public void Dispose()
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