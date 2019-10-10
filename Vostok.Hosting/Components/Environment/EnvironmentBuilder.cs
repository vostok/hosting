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
    internal class EnvironmentBuilder : IVostokEnvironmentBuilder
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
        private readonly ConfigurationBuilder configurationBuilder;

        private EnvironmentBuilder()
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
            configurationBuilder = new ConfigurationBuilder();
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
            applicationIdentitySetup(applicationIdentityBuilder);
            return this;
        }

        public IVostokEnvironmentBuilder SetupTracer(Action<IVostokTracerBuilder> tracerSetup)
        {
            tracerSetup(tracerBuilder);
            return this;
        }

        public IVostokEnvironmentBuilder SetupMetrics(Action<IVostokMetricsBuilder> metricsSetup)
        {
            metricsSetup(metricsBuilder);
            return this;
        }

        public IVostokEnvironmentBuilder SetupClusterClientSetup(Action<IVostokClusterClientSetupBuilder> clusterClientSetup)
        {
            clusterClientSetup(clusterClientSetupBuilder);
            return this;
        }

        public IVostokEnvironmentBuilder SetupZooKeeperClient(Action<IVostokZooKeeperClientBuilder> zooKeeperClientSetup)
        {
            zooKeeperClientSetup(zooKeeperClientBuilder);
            return this;
        }

        public IVostokEnvironmentBuilder SetupHerculesSink(Action<IVostokHerculesSinkBuilder> sinkSetup)
        {
            sinkSetup(herculesSinkBuilder);
            return this;
        }

        public IVostokEnvironmentBuilder SetupServiceBeacon(Action<IVostokServiceBeaconBuilder> serviceBeaconSetup)
        {
            serviceBeaconSetup(serviceBeaconBuilder);
            return this;
        }

        public IVostokEnvironmentBuilder SetupServiceLocator(Action<IVostokServiceLocatorBuilder> serviceLocatorSetup)
        {
            serviceLocatorSetup(serviceLocatorBuilder);
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

            context.ApplicationIdentity = applicationIdentityBuilder.Build(context);
            Substitute(context);

            context.ClusterConfigClient = clusterConfigClientBuilder.Build(context);
            Substitute(context);

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