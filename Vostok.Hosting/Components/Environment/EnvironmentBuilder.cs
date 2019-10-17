using System;
using System.Threading;
using Vostok.Clusterclient.Core;
using Vostok.Context;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Components.ApplicationIdentity;
using Vostok.Hosting.Components.ClusterClient;
using Vostok.Hosting.Components.Configuration;
using Vostok.Hosting.Components.Hercules;
using Vostok.Hosting.Components.HostExtensions;
using Vostok.Hosting.Components.Log;
using Vostok.Hosting.Components.Metrics;
using Vostok.Hosting.Components.ServiceDiscovery;
using Vostok.Hosting.Components.Tracing;
using Vostok.Hosting.Components.ZooKeeper;
using Vostok.Hosting.Helpers;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.ServiceDiscovery.Abstractions;
using Vostok.Tracing;
using Vostok.Tracing.Abstractions;
using Vostok.ZooKeeper.Client.Abstractions;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Environment
{
    internal class EnvironmentBuilder : IVostokHostingEnvironmentBuilder
    {
        private readonly ConfigurationBuilder configurationBuilder;
        private readonly ClusterConfigClientBuilder clusterConfigClientBuilder;

        private readonly CustomizableBuilder<LogsBuilder, Logs> compositeLogBuilder;
        private readonly CustomizableBuilder<ApplicationIdentityBuilder, IVostokApplicationIdentity> applicationIdentityBuilder;
        private readonly CustomizableBuilder<HerculesSinkBuilder, IHerculesSink> herculesSinkBuilder;
        private readonly CustomizableBuilder<TracerBuilder, (ITracer, TracerSettings)> tracerBuilder;
        private readonly CustomizableBuilder<ClusterClientSetupBuilder, ClusterClientSetup> clusterClientSetupBuilder;
        private readonly CustomizableBuilder<MetricsBuilder, IVostokApplicationMetrics> metricsBuilder;
        private readonly CustomizableBuilder<ZooKeeperClientBuilder, IZooKeeperClient> zooKeeperClientBuilder;
        private readonly CustomizableBuilder<ServiceBeaconBuilder, IServiceBeacon> serviceBeaconBuilder;
        private readonly CustomizableBuilder<ServiceLocatorBuilder, IServiceLocator> serviceLocatorBuilder;
        private readonly HostExtensionsBuilder hostExtensionsBuilder;

        private EnvironmentBuilder()
        {
            configurationBuilder = new ConfigurationBuilder();
            clusterConfigClientBuilder = new ClusterConfigClientBuilder();

            compositeLogBuilder = new CustomizableBuilder<LogsBuilder, Logs>(new LogsBuilder());
            applicationIdentityBuilder = new CustomizableBuilder<ApplicationIdentityBuilder, IVostokApplicationIdentity>(new ApplicationIdentityBuilder());
            herculesSinkBuilder = new CustomizableBuilder<HerculesSinkBuilder, IHerculesSink>(new HerculesSinkBuilder());
            tracerBuilder = new CustomizableBuilder<TracerBuilder, (ITracer, TracerSettings)>(new TracerBuilder());
            clusterClientSetupBuilder = new CustomizableBuilder<ClusterClientSetupBuilder, ClusterClientSetup>(new ClusterClientSetupBuilder());
            metricsBuilder = new CustomizableBuilder<MetricsBuilder, IVostokApplicationMetrics>(new MetricsBuilder());
            zooKeeperClientBuilder = new CustomizableBuilder<ZooKeeperClientBuilder, IZooKeeperClient>(new ZooKeeperClientBuilder());
            serviceBeaconBuilder = new CustomizableBuilder<ServiceBeaconBuilder, IServiceBeacon>(new ServiceBeaconBuilder());
            serviceLocatorBuilder = new CustomizableBuilder<ServiceLocatorBuilder, IServiceLocator>(new ServiceLocatorBuilder());
            hostExtensionsBuilder = new HostExtensionsBuilder();
        }

        public static VostokHostingEnvironment Build(VostokHostingEnvironmentSetup setup, CancellationToken shutdownToken)
        {
            var builder = new EnvironmentBuilder();
            setup(builder);
            return builder.Build(shutdownToken);
        }

        private VostokHostingEnvironment Build(CancellationToken shutdownToken)
        {
            var context = new BuildContext {ShutdownToken = shutdownToken};

            try
            {
                return BuildInner(context);
            }
            catch (Exception error)
            {
                context.Log.Error(error, "Failed to build vostok hosting environment.");
                context.PrintBufferedLogs();

                context.Dispose();

                throw;
            }
        }

        private VostokHostingEnvironment BuildInner(BuildContext context)
        {
            var clusterConfigClient = clusterConfigClientBuilder.Build(context);
            context.ClusterConfigClient = clusterConfigClient;

            (context.ConfigurationSource, context.ConfigurationProvider) = configurationBuilder.Build(context);
            context.SetupContext = new EnvironmentSetupContext(context.Log, context.ConfigurationSource, context.ConfigurationProvider, context.ClusterConfigClient);

            context.ApplicationIdentity = applicationIdentityBuilder.Build(context);

            context.ZooKeeperClient = zooKeeperClientBuilder.Build(context);

            context.ServiceLocator = serviceLocatorBuilder.Build(context);

            context.HerculesSink = herculesSinkBuilder.Build(context);

            context.Logs = compositeLogBuilder.Build(context);
            if (context.Logs.Count() == 0)
            {
                context.Log.LogDisabled("All logs");
                context.PrintBufferedLogs();
            }

            context.Log = context.Logs.BuildCompositeLog();

            context.SubstituteTracer(tracerBuilder.Build(context));

            context.Metrics = metricsBuilder.Build(context);
            if (context.HerculesSink != null)
                HerculesSinkMetrics.Measure(context.Metrics, context.HerculesSink);

            FlowingContext.Configuration.ErrorCallback = (errorMessage, error) => context.Log.Error(error, errorMessage);

            context.ServiceBeacon = serviceBeaconBuilder.Build(context);

            var vostokHostingEnvironment = new VostokHostingEnvironment(
                context.ShutdownToken,
                context.ApplicationIdentity,
                context.Metrics,
                context.Log,
                context.Tracer,
                context.HerculesSink ?? new DevNullHerculesSink(),
                context.ConfigurationSource,
                context.ConfigurationProvider,
                context.ClusterConfigClient,
                context.ServiceBeacon,
                context.ServiceLocator,
                FlowingContext.Globals,
                FlowingContext.Properties,
                FlowingContext.Configuration,
                clusterClientSetupBuilder.Build(context),
                hostExtensionsBuilder.HostExtensions,
                context.Dispose);

            hostExtensionsBuilder.Build(context, vostokHostingEnvironment);

            return vostokHostingEnvironment;
        }

        #region SetupComponents

        public IVostokHostingEnvironmentBuilder SetupLog(Action<IVostokCompositeLogBuilder> compositeLogSetup)
        {
            compositeLogBuilder.AddCustomization(compositeLogSetup);
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupLog(Action<IVostokCompositeLogBuilder, IVostokHostingEnvironmentSetupContext> compositeLogSetup)
        {
            compositeLogBuilder.AddCustomization(compositeLogSetup);
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupClusterConfigClient(Action<IVostokClusterConfigClientBuilder> clusterConfigClientSetup)
        {
            clusterConfigClientSetup(clusterConfigClientBuilder);
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupApplicationIdentity(Action<IVostokApplicationIdentityBuilder> applicationIdentitySetup)
        {
            applicationIdentityBuilder.AddCustomization(applicationIdentitySetup);
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupApplicationIdentity(Action<IVostokApplicationIdentityBuilder, IVostokHostingEnvironmentSetupContext> applicationIdentitySetup)
        {
            applicationIdentityBuilder.AddCustomization(applicationIdentitySetup);
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupTracer(Action<IVostokTracerBuilder> tracerSetup)
        {
            tracerBuilder.AddCustomization(tracerSetup);
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupTracer(Action<IVostokTracerBuilder, IVostokHostingEnvironmentSetupContext> tracerSetup)
        {
            tracerBuilder.AddCustomization(tracerSetup);
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupMetrics(Action<IVostokMetricsBuilder> metricsSetup)
        {
            metricsBuilder.AddCustomization(metricsSetup);
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupMetrics(Action<IVostokMetricsBuilder, IVostokHostingEnvironmentSetupContext> metricsSetup)
        {
            metricsBuilder.AddCustomization(metricsSetup);
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupClusterClientSetup(Action<IVostokClusterClientSetupBuilder> clusterClientSetup)
        {
            clusterClientSetupBuilder.AddCustomization(clusterClientSetup);
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupClusterClientSetup(Action<IVostokClusterClientSetupBuilder, IVostokHostingEnvironmentSetupContext> clusterClientSetup)
        {
            clusterClientSetupBuilder.AddCustomization(clusterClientSetup);
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupZooKeeperClient(Action<IVostokZooKeeperClientBuilder> zooKeeperClientSetup)
        {
            zooKeeperClientBuilder.AddCustomization(zooKeeperClientSetup);
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupZooKeeperClient(Action<IVostokZooKeeperClientBuilder, IVostokHostingEnvironmentSetupContext> zooKeeperClientSetup)
        {
            zooKeeperClientBuilder.AddCustomization(zooKeeperClientSetup);
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupHerculesSink(Action<IVostokHerculesSinkBuilder> herculesSinkSetup)
        {
            herculesSinkBuilder.AddCustomization(b => b.Enable());
            herculesSinkBuilder.AddCustomization(herculesSinkSetup);
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupHerculesSink(Action<IVostokHerculesSinkBuilder, IVostokHostingEnvironmentSetupContext> herculesSinkSetup)
        {
            herculesSinkBuilder.AddCustomization(b => b.Enable());
            herculesSinkBuilder.AddCustomization(herculesSinkSetup);
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupServiceBeacon()
        {
            serviceBeaconBuilder.AddCustomization(b => b.Enable());
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupServiceBeacon(Action<IVostokServiceBeaconBuilder> serviceBeaconSetup)
        {
            serviceBeaconBuilder.AddCustomization(b => b.Enable());
            serviceBeaconBuilder.AddCustomization(serviceBeaconSetup);
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupServiceBeacon(Action<IVostokServiceBeaconBuilder, IVostokHostingEnvironmentSetupContext> serviceBeaconSetup)
        {
            serviceBeaconBuilder.AddCustomization(b => b.Enable());
            serviceBeaconBuilder.AddCustomization(serviceBeaconSetup);
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupServiceLocator(Action<IVostokServiceLocatorBuilder> serviceLocatorSetup)
        {
            serviceLocatorBuilder.AddCustomization(serviceLocatorSetup);
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupServiceLocator(Action<IVostokServiceLocatorBuilder, IVostokHostingEnvironmentSetupContext> serviceLocatorSetup)
        {
            serviceLocatorBuilder.AddCustomization(serviceLocatorSetup);
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupHostExtensions(Action<IVostokHostExtensionsBuilder> hostExtensionsSetup)
        {
            hostExtensionsBuilder.AddCustomization(hostExtensionsSetup);
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupHostExtensions(Action<IVostokHostExtensionsBuilder, IVostokHostingEnvironment> hostExtensionsSetup)
        {
            hostExtensionsBuilder.AddCustomization(hostExtensionsSetup);
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupConfiguration(Action<IVostokConfigurationBuilder> configurationSetup)
        {
            configurationSetup(configurationBuilder);
            return this;
        }

        #endregion
    }
}