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
        private readonly ConfigurationBuilder configurationBuilder;
        private readonly ClusterConfigClientBuilder clusterConfigClientBuilder;

        private readonly CustomizableBuilder<CompositeLogBuilder, ILog> compositeLogBuilder;
        private readonly CustomizableBuilder<ApplicationIdentityBuilder, IVostokApplicationIdentity> applicationIdentityBuilder;
        private readonly CustomizableBuilder<HerculesSinkBuilder, IHerculesSink> herculesSinkBuilder;
        private readonly CustomizableBuilder<TracerBuilder, ITracer> tracerBuilder;
        private readonly CustomizableBuilder<ClusterClientSetupBuilder, ClusterClientSetup> clusterClientSetupBuilder;
        private readonly CustomizableBuilder<MetricsBuilder, IVostokApplicationMetrics> metricsBuilder;
        private readonly CustomizableBuilder<ZooKeeperClientBuilder, IZooKeeperClient> zooKeeperClientBuilder;
        private readonly CustomizableBuilder<ServiceBeaconBuilder, IServiceBeacon> serviceBeaconBuilder;
        private readonly CustomizableBuilder<ServiceLocatorBuilder, IServiceLocator> serviceLocatorBuilder;

        private EnvironmentBuilder()
        {
            configurationBuilder = new ConfigurationBuilder();
            clusterConfigClientBuilder = new ClusterConfigClientBuilder();

            compositeLogBuilder = new CustomizableBuilder<CompositeLogBuilder, ILog>(new CompositeLogBuilder());
            applicationIdentityBuilder = new CustomizableBuilder<ApplicationIdentityBuilder, IVostokApplicationIdentity>(new ApplicationIdentityBuilder());
            herculesSinkBuilder = new CustomizableBuilder<HerculesSinkBuilder, IHerculesSink>(new HerculesSinkBuilder());
            tracerBuilder = new CustomizableBuilder<TracerBuilder, ITracer>(new TracerBuilder());
            clusterClientSetupBuilder = new CustomizableBuilder<ClusterClientSetupBuilder, ClusterClientSetup>(new ClusterClientSetupBuilder());
            metricsBuilder = new CustomizableBuilder<MetricsBuilder, IVostokApplicationMetrics>(new MetricsBuilder());
            zooKeeperClientBuilder = new CustomizableBuilder<ZooKeeperClientBuilder, IZooKeeperClient>(new ZooKeeperClientBuilder());;
            serviceBeaconBuilder = new CustomizableBuilder<ServiceBeaconBuilder, IServiceBeacon>(new ServiceBeaconBuilder());
            serviceLocatorBuilder = new CustomizableBuilder<ServiceLocatorBuilder, IServiceLocator>(new ServiceLocatorBuilder());
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
            compositeLogBuilder.AddCustomization(compositeLogSetup);
            return this;
        }

        public IVostokEnvironmentBuilder SetupLog(Action<IVostokCompositeLogBuilder, IVostokConfigurationContext> compositeLogSetup)
        {
            compositeLogBuilder.AddCustomization(compositeLogSetup);
            return this;
        }

        public IVostokEnvironmentBuilder SetupClusterConfigClient(Action<IVostokClusterConfigClientBuilder> clusterConfigClientSetup)
        {
            clusterConfigClientSetup(clusterConfigClientBuilder);
            return this;
        }

        public IVostokEnvironmentBuilder SetupApplicationIdentity(Action<IVostokApplicationIdentityBuilder> applicationIdentitySetup)
        {
            applicationIdentityBuilder.AddCustomization(applicationIdentitySetup);
            return this;
        }

        public IVostokEnvironmentBuilder SetupApplicationIdentity(Action<IVostokApplicationIdentityBuilder, IVostokConfigurationContext> applicationIdentitySetup)
        {
            applicationIdentityBuilder.AddCustomization(applicationIdentitySetup);
            return this;
        }

        public IVostokEnvironmentBuilder SetupTracer(Action<IVostokTracerBuilder> tracerSetup)
        {
            tracerBuilder.AddCustomization(tracerSetup);
            return this;
        }

        public IVostokEnvironmentBuilder SetupTracer(Action<IVostokTracerBuilder, IVostokConfigurationContext> tracerSetup)
        {
            tracerBuilder.AddCustomization(tracerSetup);
            return this;
        }

        public IVostokEnvironmentBuilder SetupMetrics(Action<IVostokMetricsBuilder> metricsSetup)
        {
            metricsBuilder.AddCustomization(metricsSetup);
            return this;
        }

        public IVostokEnvironmentBuilder SetupMetrics(Action<IVostokMetricsBuilder, IVostokConfigurationContext> metricsSetup)
        {
            metricsBuilder.AddCustomization(metricsSetup);
            return this;
        }

        public IVostokEnvironmentBuilder SetupClusterClientSetup(Action<IVostokClusterClientSetupBuilder> clusterClientSetup)
        {
            clusterClientSetupBuilder.AddCustomization(clusterClientSetup);
            return this;
        }

        public IVostokEnvironmentBuilder SetupClusterClientSetup(Action<IVostokClusterClientSetupBuilder, IVostokConfigurationContext> clusterClientSetup)
        {
            clusterClientSetupBuilder.AddCustomization(clusterClientSetup);
            return this;
        }

        public IVostokEnvironmentBuilder SetupZooKeeperClient(Action<IVostokZooKeeperClientBuilder> zooKeeperClientSetup)
        {
            zooKeeperClientBuilder.AddCustomization(zooKeeperClientSetup);
            return this;
        }

        public IVostokEnvironmentBuilder SetupZooKeeperClient(Action<IVostokZooKeeperClientBuilder, IVostokConfigurationContext> zooKeeperClientSetup)
        {
            zooKeeperClientBuilder.AddCustomization(zooKeeperClientSetup);
            return this;
        }

        public IVostokEnvironmentBuilder SetupHerculesSink(Action<IVostokHerculesSinkBuilder> herculesSinkSetup)
        {
            herculesSinkBuilder.AddCustomization(herculesSinkSetup);
            return this;
        }

        public IVostokEnvironmentBuilder SetupHerculesSink(Action<IVostokHerculesSinkBuilder, IVostokConfigurationContext> herculesSinkSetup)
        {
            herculesSinkBuilder.AddCustomization(herculesSinkSetup);
            return this;
        }

        public IVostokEnvironmentBuilder SetupServiceBeacon(Action<IVostokServiceBeaconBuilder> serviceBeaconSetup)
        {
            serviceBeaconBuilder.AddCustomization(serviceBeaconSetup);
            return this;
        }

        public IVostokEnvironmentBuilder SetupServiceBeacon(Action<IVostokServiceBeaconBuilder, IVostokConfigurationContext> serviceBeaconSetup)
        {
            serviceBeaconBuilder.AddCustomization(serviceBeaconSetup);
            return this;
        }

        public IVostokEnvironmentBuilder SetupServiceLocator(Action<IVostokServiceLocatorBuilder> serviceLocatorSetup)
        {
            serviceLocatorBuilder.AddCustomization(serviceLocatorSetup);
            return this;
        }

        public IVostokEnvironmentBuilder SetupServiceLocator(Action<IVostokServiceLocatorBuilder, IVostokConfigurationContext> serviceLocatorSetup)
        {
            serviceLocatorBuilder.AddCustomization(serviceLocatorSetup);
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
            context.ConfigurationContext = new ConfigurationContext(context.ConfigurationSource, context.ConfigurationProvider, context.ClusterConfigClient);

            context.ApplicationIdentity = applicationIdentityBuilder.Build(context);

            context.ZooKeeperClient = zooKeeperClientBuilder.Build(context);

            context.ServiceLocator = serviceLocatorBuilder.Build(context);

            context.HerculesSink = herculesSinkBuilder.Build(context);
            HerculesSinkProvider.Configure(context.HerculesSink, true);

            context.Log = compositeLogBuilder.Build(context);
            context.Tracer = tracerBuilder.Build(context);

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
    }
}