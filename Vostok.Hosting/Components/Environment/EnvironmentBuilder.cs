using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Vostok.ClusterConfig.Client;
using Vostok.ClusterConfig.Client.Abstractions;
using Vostok.Commons.Time;
using Vostok.Configuration;
using Vostok.Configuration.Abstractions;
using Vostok.Context;
using Vostok.Datacenters;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Components.Application;
using Vostok.Hosting.Components.Configuration;
using Vostok.Hosting.Components.Datacenters;
using Vostok.Hosting.Components.Hercules;
using Vostok.Hosting.Components.HostExtensions;
using Vostok.Hosting.Components.Log;
using Vostok.Hosting.Components.Metrics;
using Vostok.Hosting.Components.ServiceDiscovery;
using Vostok.Hosting.Components.Tracing;
using Vostok.Hosting.Components.ZooKeeper;
using Vostok.Hosting.Helpers;
using Vostok.Hosting.Models;
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
        private readonly VostokHostingEnvironmentFactorySettings settings;

        private readonly CustomizableBuilder<ConfigurationBuilder, (IConfigurationSource source, IConfigurationSource secretSource, IConfigurationProvider provider, IConfigurationProvider secretProvider)> configurationBuilder;
        private readonly CustomizableBuilder<ClusterConfigClientBuilder, IClusterConfigClient> clusterConfigClientBuilder;
        private readonly CustomizableBuilder<LogsBuilder, Logs> compositeLogBuilder;
        private readonly CustomizableBuilder<ApplicationIdentityBuilder, IVostokApplicationIdentity> applicationIdentityBuilder;
        private readonly CustomizableBuilder<ApplicationLimitsBuilder, IVostokApplicationLimits> applicationLimitsBuilder;
        private readonly CustomizableBuilder<ApplicationReplicationInfoBuilder, Func<IVostokApplicationReplicationInfo>> applicationReplicationInfoBuilder;
        private readonly CustomizableBuilder<HerculesSinkBuilder, IHerculesSink> herculesSinkBuilder;
        private readonly CustomizableBuilder<TracerBuilder, (ITracer, TracerSettings)> tracerBuilder;
        private readonly CustomizableBuilder<DatacentersBuilder, IDatacenters> datacentersBuilder;
        private readonly CustomizableBuilder<MetricsBuilder, IVostokApplicationMetrics> metricsBuilder;
        private readonly CustomizableBuilder<ZooKeeperClientBuilder, IZooKeeperClient> zooKeeperClientBuilder;
        private readonly CustomizableBuilder<ServiceBeaconBuilder, IServiceBeacon> serviceBeaconBuilder;
        private readonly CustomizableBuilder<ServiceLocatorBuilder, IServiceLocator> serviceLocatorBuilder;
        private readonly IntermediateApplicationIdentityBuilder intermediateApplicationIdentityBuilder;
        private readonly HostExtensionsBuilder hostExtensionsBuilder;

        private List<CancellationToken> shutdownTokens;
        private TimeSpan shutdownTimeout;

        private EnvironmentBuilder(VostokHostingEnvironmentFactorySettings settings)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));

            shutdownTokens = new List<CancellationToken>();
            shutdownTimeout = 5.Seconds();
            configurationBuilder = new CustomizableBuilder<ConfigurationBuilder, (IConfigurationSource source, IConfigurationSource secretSource, IConfigurationProvider provider, IConfigurationProvider secretProvider)>(new ConfigurationBuilder());
            clusterConfigClientBuilder = new CustomizableBuilder<ClusterConfigClientBuilder, IClusterConfigClient>(new ClusterConfigClientBuilder());
            compositeLogBuilder = new CustomizableBuilder<LogsBuilder, Logs>(new LogsBuilder());
            applicationIdentityBuilder = new CustomizableBuilder<ApplicationIdentityBuilder, IVostokApplicationIdentity>(new ApplicationIdentityBuilder());
            applicationLimitsBuilder = new CustomizableBuilder<ApplicationLimitsBuilder, IVostokApplicationLimits>(new ApplicationLimitsBuilder());
            applicationReplicationInfoBuilder = new CustomizableBuilder<ApplicationReplicationInfoBuilder, Func<IVostokApplicationReplicationInfo>>(new ApplicationReplicationInfoBuilder());
            herculesSinkBuilder = new CustomizableBuilder<HerculesSinkBuilder, IHerculesSink>(new HerculesSinkBuilder());
            tracerBuilder = new CustomizableBuilder<TracerBuilder, (ITracer, TracerSettings)>(new TracerBuilder());
            datacentersBuilder = new CustomizableBuilder<DatacentersBuilder, IDatacenters>(new DatacentersBuilder());
            metricsBuilder = new CustomizableBuilder<MetricsBuilder, IVostokApplicationMetrics>(new MetricsBuilder());
            zooKeeperClientBuilder = new CustomizableBuilder<ZooKeeperClientBuilder, IZooKeeperClient>(new ZooKeeperClientBuilder());
            serviceBeaconBuilder = new CustomizableBuilder<ServiceBeaconBuilder, IServiceBeacon>(new ServiceBeaconBuilder());
            serviceLocatorBuilder = new CustomizableBuilder<ServiceLocatorBuilder, IServiceLocator>(new ServiceLocatorBuilder());
            intermediateApplicationIdentityBuilder = new IntermediateApplicationIdentityBuilder();
            hostExtensionsBuilder = new HostExtensionsBuilder();
        }

        public static VostokHostingEnvironment Build(VostokHostingEnvironmentSetup setup, VostokHostingEnvironmentFactorySettings settings)
        {
            var builder = new EnvironmentBuilder(settings);
            setup(builder);
            return builder.Build();
        }

        private VostokHostingEnvironment Build()
        {
            var context = new BuildContext();

            try
            {
                return BuildInner(context);
            }
            catch (Exception error)
            {
                context.Log.ForContext<VostokHostingEnvironment>().Error(error, "Failed to build hosting environment.");
                context.PrintBufferedLogs();
                context.Dispose();

                throw;
            }
        }

        private VostokHostingEnvironment BuildInner(BuildContext context)
        {
            if (settings.ConfigureStaticProviders)
            {
                LogProvider.Configure(context.Log, true);
                TracerProvider.Configure(context.Tracer, true);
            }

            context.ClusterConfigClient = clusterConfigClientBuilder.Build(context);

            if (settings.ConfigureStaticProviders && context.ClusterConfigClient is ClusterConfigClient ccClient)
                ClusterConfigClient.TrySetDefaultClient(ccClient);

            (context.ConfigurationSource,
                context.SecretConfigurationSource,
                context.ConfigurationProvider,
                context.SecretConfigurationProvider) = configurationBuilder.Build(context);

            if (settings.ConfigureStaticProviders && context.ConfigurationProvider is ConfigurationProvider configProvider)
                ConfigurationProvider.TrySetDefault(configProvider);

            context.SetupContext = new EnvironmentSetupContext(
                context.Log,
                context.ConfigurationSource,
                context.SecretConfigurationSource,
                context.ConfigurationProvider,
                context.SecretConfigurationProvider,
                context.ClusterConfigClient);

            context.Datacenters = datacentersBuilder.Build(context);
            if (settings.ConfigureStaticProviders && context.Datacenters != null)
                DatacentersProvider.Configure(context.Datacenters, true);

            context.ApplicationIdentity = applicationIdentityBuilder.Build(context);

            context.ZooKeeperClient = zooKeeperClientBuilder.Build(context);

            context.ServiceLocator = serviceLocatorBuilder.Build(context);

            context.HerculesSink = herculesSinkBuilder.Build(context);

            if (settings.ConfigureStaticProviders && context.HerculesSink != null)
                HerculesSinkProvider.Configure(context.HerculesSink, true);

            context.Logs = compositeLogBuilder.Build(context);
            var hasLogs = context.Logs.Count() > 0;
            if (hasLogs)
                context.Log = context.Logs.BuildCompositeLog();

            context.ServiceBeacon = serviceBeaconBuilder.Build(context);

            context.SubstituteTracer(tracerBuilder.Build(context));

            context.Metrics = metricsBuilder.Build(context);

            HerculesSinkMetrics.Measure(context.HerculesSink, context.Metrics, context.Log);

            if (settings.ConfigureStaticProviders)
                FlowingContext.Configuration.ErrorCallback = (errorMessage, error) => context.Log.ForContext(typeof(FlowingContext)).Error(error, errorMessage);

            context.ServiceBeacon.ReplicaInfo.TryGetUrl(out var url);

            var vostokHostingEnvironment = new VostokHostingEnvironment(
                shutdownTokens.Any() ? CancellationTokenSource.CreateLinkedTokenSource(shutdownTokens.ToArray()).Token : default,
                shutdownTimeout,
                context.ApplicationIdentity,
                applicationLimitsBuilder.Build(context),
                applicationReplicationInfoBuilder.Build(context),
                context.Metrics,
                context.Log,
                context.Tracer,
                context.HerculesSink ?? new DevNullHerculesSink(),
                context.ConfigurationSource,
                context.SecretConfigurationSource,
                context.ConfigurationProvider,
                context.SecretConfigurationProvider,
                context.ClusterConfigClient,
                context.ServiceBeacon,
                url?.Port,
                context.ServiceLocator,
                FlowingContext.Globals,
                FlowingContext.Properties,
                FlowingContext.Configuration,
                context.Datacenters ?? new EmptyDatacenters(),
                hostExtensionsBuilder.HostExtensions,
                context.Dispose);

            hostExtensionsBuilder.Build(context, vostokHostingEnvironment);

            if (!hasLogs)
            {
                context.LogDisabled("All logs");
                context.PrintBufferedLogs();
                context.Log = context.Logs.BuildCompositeLog();
            }

            return vostokHostingEnvironment;
        }

        #region SetupComponents

        public IVostokHostingEnvironmentBuilder SetupShutdownToken(CancellationToken shutdownToken)
        {
            shutdownTokens.Add(shutdownToken);
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupShutdownTimeout(TimeSpan shutdownTimeout)
        {
            this.shutdownTimeout = shutdownTimeout.Cut(100.Milliseconds(), 0.05);
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupLog(Action<IVostokCompositeLogBuilder> setup)
        {
            compositeLogBuilder.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupLog(Action<IVostokCompositeLogBuilder, IVostokHostingEnvironmentSetupContext> setup)
        {
            compositeLogBuilder.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupClusterConfigClient(Action<IVostokClusterConfigClientBuilder> setup)
        {
            clusterConfigClientBuilder.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupClusterConfigClient(Action<IVostokClusterConfigClientBuilder, IVostokApplicationIdentity> setup)
        {
            clusterConfigClientBuilder.AddCustomization(builder => setup(builder, intermediateApplicationIdentityBuilder.Build(null)));
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupApplicationIdentity(Action<IVostokApplicationIdentityBuilder> setup)
        {
            applicationIdentityBuilder.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            setup(intermediateApplicationIdentityBuilder);
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupApplicationIdentity(Action<IVostokApplicationIdentityBuilder, IVostokHostingEnvironmentSetupContext> setup)
        {
            applicationIdentityBuilder.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupApplicationLimits(Action<IVostokApplicationLimitsBuilder> setup)
        {
            applicationLimitsBuilder.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupApplicationLimits(Action<IVostokApplicationLimitsBuilder, IVostokHostingEnvironmentSetupContext> setup)
        {
            applicationLimitsBuilder.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupApplicationReplicationInfo(Action<IVostokApplicationReplicationInfoBuilder> setup)
        {
            applicationReplicationInfoBuilder.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupApplicationReplicationInfo(Action<IVostokApplicationReplicationInfoBuilder, IVostokHostingEnvironmentSetupContext> setup)
        {
            applicationReplicationInfoBuilder.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupTracer(Action<IVostokTracerBuilder> setup)
        {
            tracerBuilder.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupTracer(Action<IVostokTracerBuilder, IVostokHostingEnvironmentSetupContext> setup)
        {
            tracerBuilder.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupMetrics(Action<IVostokMetricsBuilder> setup)
        {
            metricsBuilder.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupMetrics(Action<IVostokMetricsBuilder, IVostokHostingEnvironmentSetupContext> setup)
        {
            metricsBuilder.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupDatacenters(Action<IVostokDatacentersBuilder> setup)
        {
            datacentersBuilder.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupDatacenters(Action<IVostokDatacentersBuilder, IVostokHostingEnvironmentSetupContext> setup)
        {
            datacentersBuilder.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupZooKeeperClient(Action<IVostokZooKeeperClientBuilder> setup)
        {
            zooKeeperClientBuilder.AddCustomization(b => b.Enable());
            zooKeeperClientBuilder.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupZooKeeperClient(Action<IVostokZooKeeperClientBuilder, IVostokHostingEnvironmentSetupContext> setup)
        {
            zooKeeperClientBuilder.AddCustomization(b => b.Enable());
            zooKeeperClientBuilder.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupHerculesSink(Action<IVostokHerculesSinkBuilder> setup)
        {
            herculesSinkBuilder.AddCustomization(b => b.Enable());
            herculesSinkBuilder.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupHerculesSink(Action<IVostokHerculesSinkBuilder, IVostokHostingEnvironmentSetupContext> setup)
        {
            herculesSinkBuilder.AddCustomization(b => b.Enable());
            herculesSinkBuilder.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupServiceBeacon(Action<IVostokServiceBeaconBuilder> setup)
        {
            serviceBeaconBuilder.AddCustomization(b => b.Enable());
            serviceBeaconBuilder.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupServiceBeacon(Action<IVostokServiceBeaconBuilder, IVostokHostingEnvironmentSetupContext> setup)
        {
            serviceBeaconBuilder.AddCustomization(b => b.Enable());
            serviceBeaconBuilder.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupServiceLocator(Action<IVostokServiceLocatorBuilder> setup)
        {
            serviceLocatorBuilder.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupServiceLocator(Action<IVostokServiceLocatorBuilder, IVostokHostingEnvironmentSetupContext> setup)
        {
            serviceLocatorBuilder.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupHostExtensions(Action<IVostokHostExtensionsBuilder> setup)
        {
            hostExtensionsBuilder.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupHostExtensions(Action<IVostokHostExtensionsBuilder, IVostokHostingEnvironment> setup)
        {
            hostExtensionsBuilder.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupConfiguration(Action<IVostokConfigurationBuilder> setup)
        {
            configurationBuilder.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupConfiguration(Action<IVostokConfigurationBuilder, IVostokApplicationIdentity> setup)
        {
            configurationBuilder.AddCustomization(builder => setup(builder, intermediateApplicationIdentityBuilder.Build(null)));
            return this;
        }

        #endregion
    }
}