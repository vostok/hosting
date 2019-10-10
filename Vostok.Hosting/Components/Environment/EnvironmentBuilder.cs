using System;
using System.Threading;
using Vostok.Clusterclient.Core;
using Vostok.ClusterConfig.Client.Abstractions;
using Vostok.Context;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Components.ApplicationIdentity;
using Vostok.Hosting.Components.ClusterClient;
using Vostok.Hosting.Components.Configuration;
using Vostok.Hosting.Components.Hercules;
using Vostok.Hosting.Components.Log;
using Vostok.Hosting.Components.Metrics;
using Vostok.Hosting.Components.ServiceDiscovery;
using Vostok.Hosting.Components.Tracing;
using Vostok.Hosting.Components.ZooKeeper;
using Vostok.Hosting.Helpers;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.ServiceDiscovery.Abstractions;
using Vostok.Tracing.Abstractions;
using Vostok.ZooKeeper.Client.Abstractions;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Environment
{
    internal class EnvironmentBuilder : IVostokEnvironmentBuilder
    {
        private readonly CompositeLogBuilder compositeLogBuilder;
        private readonly ConfigurationBuilder configurationBuilder;
        private readonly ClusterConfigClientBuilder clusterConfigClientBuilder;

        private readonly BuilderWithSetup<ApplicationIdentityBuilder, IVostokApplicationIdentity> applicationIdentityBuilder;
        private readonly BuilderWithSetup<HerculesSinkBuilder, IHerculesSink> herculesSinkBuilder;
        private readonly BuilderWithSetup<TracerBuilder, ITracer> tracerBuilder;
        private readonly BuilderWithSetup<ClusterClientSetupBuilder, ClusterClientSetup> clusterClientSetupBuilder;
        private readonly BuilderWithSetup<MetricsBuilder, IVostokApplicationMetrics> metricsBuilder;
        private readonly BuilderWithSetup<ZooKeeperClientBuilder, IZooKeeperClient> zooKeeperClientBuilder;
        private readonly BuilderWithSetup<ServiceBeaconBuilder, IServiceBeacon> serviceBeaconBuilder;
        private readonly BuilderWithSetup<ServiceLocatorBuilder, IServiceLocator> serviceLocatorBuilder;

        private EnvironmentBuilder()
        {
            compositeLogBuilder = new CompositeLogBuilder();
            configurationBuilder = new ConfigurationBuilder();
            clusterConfigClientBuilder = new ClusterConfigClientBuilder();
            
            applicationIdentityBuilder = new BuilderWithSetup<ApplicationIdentityBuilder, IVostokApplicationIdentity>(new ApplicationIdentityBuilder());
            herculesSinkBuilder = new BuilderWithSetup<HerculesSinkBuilder, IHerculesSink>(new HerculesSinkBuilder());
            tracerBuilder = new BuilderWithSetup<TracerBuilder, ITracer>(new TracerBuilder());
            clusterClientSetupBuilder = new BuilderWithSetup<ClusterClientSetupBuilder, ClusterClientSetup>(new ClusterClientSetupBuilder());
            metricsBuilder = new BuilderWithSetup<MetricsBuilder, IVostokApplicationMetrics>(new MetricsBuilder());
            zooKeeperClientBuilder = new BuilderWithSetup<ZooKeeperClientBuilder, IZooKeeperClient>(new ZooKeeperClientBuilder());;
            serviceBeaconBuilder = new BuilderWithSetup<ServiceBeaconBuilder, IServiceBeacon>(new ServiceBeaconBuilder());
            serviceLocatorBuilder = new BuilderWithSetup<ServiceLocatorBuilder, IServiceLocator>(new ServiceLocatorBuilder());
        }

        public static VostokHostingEnvironment Build(VostokHostingEnvironmentSetup setup, CancellationToken shutdownToken)
        {
            var builder = new EnvironmentBuilder();
            setup(builder);
            return builder.Build(shutdownToken);
        }

        #region SetupComponents

        public IVostokEnvironmentBuilder SetupLog(Action<IVostokCompositeLogBuilder> compositeLogSetup)
        {
            compositeLogSetup(compositeLogBuilder);
            return this;
        }

        public IVostokEnvironmentBuilder SetupClusterConfigClient(Action<IVostokClusterConfigClientBuilder> clusterConfigClientSetup)
        {
            clusterConfigClientSetup(clusterConfigClientBuilder);
            return this;
        }

        public IVostokEnvironmentBuilder SetupApplicationIdentity(Action<IVostokApplicationIdentityBuilder> applicationIdentitySetup)
        {
            applicationIdentityBuilder.Setup(applicationIdentitySetup);
            return this;
        }

        public IVostokEnvironmentBuilder SetupApplicationIdentity(Action<IVostokApplicationIdentityBuilder, IVostokConfigurationContext> applicationIdentitySetup)
        {
            applicationIdentityBuilder.Setup(applicationIdentitySetup);
            return this;
        }

        public IVostokEnvironmentBuilder SetupTracer(Action<IVostokTracerBuilder> tracerSetup)
        {
            tracerBuilder.Setup(tracerSetup);
            return this;
        }

        public IVostokEnvironmentBuilder SetupTracer(Action<IVostokTracerBuilder, IVostokConfigurationContext> tracerSetup)
        {
            tracerBuilder.Setup(tracerSetup);
            return this;
        }

        public IVostokEnvironmentBuilder SetupMetrics(Action<IVostokMetricsBuilder> metricsSetup)
        {
            metricsBuilder.Setup(metricsSetup);
            return this;
        }

        public IVostokEnvironmentBuilder SetupMetrics(Action<IVostokMetricsBuilder, IVostokConfigurationContext> metricsSetup)
        {
            metricsBuilder.Setup(metricsSetup);
            return this;
        }

        public IVostokEnvironmentBuilder SetupClusterClientSetup(Action<IVostokClusterClientSetupBuilder> clusterClientSetup)
        {
            clusterClientSetupBuilder.Setup(clusterClientSetup);
            return this;
        }

        public IVostokEnvironmentBuilder SetupClusterClientSetup(Action<IVostokClusterClientSetupBuilder, IVostokConfigurationContext> clusterClientSetup)
        {
            clusterClientSetupBuilder.Setup(clusterClientSetup);
            return this;
        }

        public IVostokEnvironmentBuilder SetupZooKeeperClient(Action<IVostokZooKeeperClientBuilder> zooKeeperClientSetup)
        {
            zooKeeperClientBuilder.Setup(zooKeeperClientSetup);
            return this;
        }

        public IVostokEnvironmentBuilder SetupZooKeeperClient(Action<IVostokZooKeeperClientBuilder, IVostokConfigurationContext> zooKeeperClientSetup)
        {
            zooKeeperClientBuilder.Setup(zooKeeperClientSetup);
            return this;
        }

        public IVostokEnvironmentBuilder SetupHerculesSink(Action<IVostokHerculesSinkBuilder> herculesSinkSetup)
        {
            herculesSinkBuilder.Setup(herculesSinkSetup);
            return this;
        }

        public IVostokEnvironmentBuilder SetupHerculesSink(Action<IVostokHerculesSinkBuilder, IVostokConfigurationContext> herculesSinkSetup)
        {
            herculesSinkBuilder.Setup(herculesSinkSetup);
            return this;
        }

        public IVostokEnvironmentBuilder SetupServiceBeacon(Action<IVostokServiceBeaconBuilder> serviceBeaconSetup)
        {
            serviceBeaconBuilder.Setup(serviceBeaconSetup);
            return this;
        }

        public IVostokEnvironmentBuilder SetupServiceBeacon(Action<IVostokServiceBeaconBuilder, IVostokConfigurationContext> serviceBeaconSetup)
        {
            serviceBeaconBuilder.Setup(serviceBeaconSetup);
            return this;
        }

        public IVostokEnvironmentBuilder SetupServiceLocator(Action<IVostokServiceLocatorBuilder> serviceLocatorSetup)
        {
            serviceLocatorBuilder.Setup(serviceLocatorSetup);
            return this;
        }

        public IVostokEnvironmentBuilder SetupServiceLocator(Action<IVostokServiceLocatorBuilder, IVostokConfigurationContext> serviceLocatorSetup)
        {
            serviceLocatorBuilder.Setup(serviceLocatorSetup);
            return this;
        }

        public IVostokEnvironmentBuilder SetupConfiguration(Action<IVostokConfigurationBuilder> configurationSetup)
        {
            configurationSetup(configurationBuilder);
            return this;
        }

        #endregion

        private VostokHostingEnvironment Build(CancellationToken shutdownToken)
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var context = new BuildContext();

            LogProvider.Configure(context.Log, true);
            TracerProvider.Configure(context.Tracer, true);

            context.ClusterConfigClient = clusterConfigClientBuilder.Build(context);
            
            (context.ConfigurationSource, context.ConfigurationProvider) = configurationBuilder.Build(context);

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
                context.ConfigurationSource,
                context.ConfigurationProvider,
                serviceBeaconBuilder.Build(context),
                context.ServiceLocator,
                FlowingContext.Globals,
                FlowingContext.Properties,
                FlowingContext.Configuration,
                clusterClientSetupBuilder.Build(context),
                null,
                DisposeEnvironment);
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