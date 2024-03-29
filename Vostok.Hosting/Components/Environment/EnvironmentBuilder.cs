﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Vostok.Clusterclient.Core;
using Vostok.ClusterConfig.Client;
using Vostok.ClusterConfig.Client.Abstractions;
using Vostok.Commons.Time;
using Vostok.Configuration;
using Vostok.Configuration.Sources;
using Vostok.Configuration.Sources.Switching;
using Vostok.Context;
using Vostok.Datacenters;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Components.Application;
using Vostok.Hosting.Components.Configuration;
using Vostok.Hosting.Components.Datacenters;
using Vostok.Hosting.Components.Diagnostics;
using Vostok.Hosting.Components.Diagnostics.InfoProviders;
using Vostok.Hosting.Components.Hercules;
using Vostok.Hosting.Components.HostExtensions;
using Vostok.Hosting.Components.Log;
using Vostok.Hosting.Components.Metrics;
using Vostok.Hosting.Components.ServiceDiscovery;
using Vostok.Hosting.Components.Shutdown;
using Vostok.Hosting.Components.SystemMetrics;
using Vostok.Hosting.Components.Tracing;
using Vostok.Hosting.Components.ZooKeeper;
using Vostok.Hosting.Helpers;
using Vostok.Hosting.Models;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.Metrics;
using Vostok.ServiceDiscovery;
using Vostok.ServiceDiscovery.Abstractions;
using Vostok.ServiceDiscovery.Telemetry;
using Vostok.Tracing;
using Vostok.Tracing.Abstractions;
using Vostok.ZooKeeper.Client.Abstractions;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Environment
{
    internal class EnvironmentBuilder : IVostokHostingEnvironmentBuilder
    {
        private static readonly object FlowingContextSync = new object();

        private readonly VostokHostingEnvironmentFactorySettings settings;

        private readonly CustomizableBuilder<ConfigurationBuilder, (SwitchingSource source, SwitchingSource secretSource, SwitchingSource mergedSource, ConfigurationProvider provider, ConfigurationProvider secretProvider)> configurationBuilder;
        private readonly CustomizableBuilder<ClusterConfigClientBuilder, IClusterConfigClient> clusterConfigClientBuilder;
        private readonly CustomizableBuilder<LogsBuilder, Logs> compositeLogBuilder;
        private readonly CustomizableBuilder<ApplicationIdentityBuilder, IVostokApplicationIdentity> applicationIdentityBuilder;
        private readonly CustomizableBuilder<ApplicationLimitsBuilder, IVostokApplicationLimits> applicationLimitsBuilder;
        private readonly CustomizableBuilder<ApplicationReplicationInfoBuilder, Func<IVostokApplicationReplicationInfo>> applicationReplicationInfoBuilder;
        private readonly CustomizableBuilder<HerculesSinkBuilder, IHerculesSink> herculesSinkBuilder;
        private readonly CustomizableBuilder<TracerBuilder, (ITracer, TracerSettings)> tracerBuilder;
        private readonly CustomizableBuilder<DatacentersBuilder, IDatacenters> datacentersBuilder;
        private readonly CustomizableBuilder<MetricsBuilder, IVostokApplicationMetrics> metricsBuilder;
        private readonly CustomizableBuilder<DiagnosticsBuilder, DiagnosticsHub> diagnosticsBuilder;
        private readonly CustomizableBuilder<ZooKeeperClientBuilder, IZooKeeperClient> zooKeeperClientBuilder;
        private readonly CustomizableBuilder<ServiceBeaconBuilder, IServiceBeacon> serviceBeaconBuilder;
        private readonly CustomizableBuilder<ServiceDiscoveryEventsContextBuilder, IServiceDiscoveryEventsContext> serviceDiscoveryEventsContextBuilder;
        private readonly CustomizableBuilder<ServiceLocatorBuilder, IServiceLocator> serviceLocatorBuilder;
        private readonly IntermediateApplicationIdentityBuilder intermediateApplicationIdentityBuilder;
        private readonly HostExtensionsBuilder hostExtensionsBuilder;
        private readonly SystemMetricsBuilder systemMetricsBuilder;

        private List<CancellationToken> shutdownTokens;
        private TimeSpan shutdownTimeout;

        private EnvironmentBuilder(VostokHostingEnvironmentFactorySettings settings)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));

            shutdownTokens = new List<CancellationToken>();
            shutdownTimeout = ShutdownConstants.DefaultShutdownTimeout;
            configurationBuilder = new CustomizableBuilder<ConfigurationBuilder, (SwitchingSource source, SwitchingSource secretSource, SwitchingSource mergedSource, ConfigurationProvider provider, ConfigurationProvider secretProvider)>(new ConfigurationBuilder());
            clusterConfigClientBuilder = new CustomizableBuilder<ClusterConfigClientBuilder, IClusterConfigClient>(new ClusterConfigClientBuilder());
            compositeLogBuilder = new CustomizableBuilder<LogsBuilder, Logs>(new LogsBuilder());
            applicationIdentityBuilder = new CustomizableBuilder<ApplicationIdentityBuilder, IVostokApplicationIdentity>(new ApplicationIdentityBuilder());
            applicationLimitsBuilder = new CustomizableBuilder<ApplicationLimitsBuilder, IVostokApplicationLimits>(new ApplicationLimitsBuilder());
            applicationReplicationInfoBuilder = new CustomizableBuilder<ApplicationReplicationInfoBuilder, Func<IVostokApplicationReplicationInfo>>(new ApplicationReplicationInfoBuilder());
            herculesSinkBuilder = new CustomizableBuilder<HerculesSinkBuilder, IHerculesSink>(new HerculesSinkBuilder());
            tracerBuilder = new CustomizableBuilder<TracerBuilder, (ITracer, TracerSettings)>(new TracerBuilder());
            datacentersBuilder = new CustomizableBuilder<DatacentersBuilder, IDatacenters>(new DatacentersBuilder());
            metricsBuilder = new CustomizableBuilder<MetricsBuilder, IVostokApplicationMetrics>(new MetricsBuilder());
            diagnosticsBuilder = new CustomizableBuilder<DiagnosticsBuilder, DiagnosticsHub>(new DiagnosticsBuilder());
            zooKeeperClientBuilder = new CustomizableBuilder<ZooKeeperClientBuilder, IZooKeeperClient>(new ZooKeeperClientBuilder());
            serviceBeaconBuilder = new CustomizableBuilder<ServiceBeaconBuilder, IServiceBeacon>(new ServiceBeaconBuilder());
            serviceDiscoveryEventsContextBuilder = new CustomizableBuilder<ServiceDiscoveryEventsContextBuilder, IServiceDiscoveryEventsContext>(new ServiceDiscoveryEventsContextBuilder());
            serviceLocatorBuilder = new CustomizableBuilder<ServiceLocatorBuilder, IServiceLocator>(new ServiceLocatorBuilder());
            intermediateApplicationIdentityBuilder = new IntermediateApplicationIdentityBuilder();
            hostExtensionsBuilder = new HostExtensionsBuilder();
            systemMetricsBuilder = new SystemMetricsBuilder();
        }

        public static VostokHostingEnvironment Build(VostokHostingEnvironmentSetup setup, VostokHostingEnvironmentFactorySettings settings)
        {
            var builder = new EnvironmentBuilder(settings);
            setup(builder);
            return builder.Build();
        }

        private VostokHostingEnvironment Build()
        {
            var context = new BuildContext(settings);

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
                DatacentersProvider.Configure(context.Datacenters, true);
            }

            context.ConfigurationSetupContext = new ConfigurationSetupContext(context.Log, context.Datacenters, () => intermediateApplicationIdentityBuilder.Build(context));

            context.ClusterConfigClient = clusterConfigClientBuilder.Build(context);

            if (settings.ConfigureStaticProviders && context.ClusterConfigClient is ClusterConfigClient ccClient)
                ClusterConfigClient.TrySetDefaultClient(ccClient);

            // note (iloktionov, 03.03.2021): FlowingContext needed because CustomizableBuilder.Build will be called before ConfigurationBuilder.Build 
            lock (FlowingContextSync)
                using (ConfigurationBuilder.UseContext(context))
                {
                    (context.ConfigurationSource,
                        context.SecretConfigurationSource,
                        context.MergedConfigurationSource,
                        context.ConfigurationProvider,
                        context.SecretConfigurationProvider) = configurationBuilder.Build(context);
                }

            if (settings.ConfigureStaticProviders && context.ConfigurationProvider is {} configProvider)
                ConfigurationProvider.TrySetDefault(configProvider);

            context.EnvironmentSetupContext = new EnvironmentSetupContext(
                context.Log,
                context.ConfigurationSource,
                context.SecretConfigurationSource,
                context.MergedConfigurationSource,
                context.ConfigurationProvider,
                context.SecretConfigurationProvider,
                context.ClusterConfigClient,
                context.Datacenters);

            var datacenters = datacentersBuilder.Build(context);
            if (datacenters != null)
                context.Datacenters = datacenters;

            context.ApplicationIdentity = applicationIdentityBuilder.Build(context);
            context.ApplicationLimits = applicationLimitsBuilder.Build(context);
            context.ApplicationReplication = applicationReplicationInfoBuilder.Build(context);

            context.ZooKeeperClient = zooKeeperClientBuilder.Build(context, out var finalZooKeeperClientBuilder);
            if (settings.ConfigureStaticProviders)
                finalZooKeeperClientBuilder.StaticProviderCustomization.Customize(context.ZooKeeperClient);

            context.ServiceLocator = serviceLocatorBuilder.Build(context, out var finalServiceLocatorBuilder);
            if (settings.ConfigureStaticProviders)
                finalServiceLocatorBuilder.StaticProviderCustomization.Customize(context.ServiceLocator);

            context.HerculesSink = herculesSinkBuilder.Build(context);

            if (settings.ConfigureStaticProviders && context.HerculesSink != null)
                HerculesSinkProvider.Configure(context.HerculesSink, true);

            context.Logs = compositeLogBuilder.Build(context);

            var hasLogs = context.Logs.Count() > 0;
            if (hasLogs)
            {
                context.Log = context.Logs.BuildCompositeLog(out var configuredLoggers);
                context.LogConfiguredLoggers(configuredLoggers);
            }

            context.ServiceDiscoveryEventsContext = serviceDiscoveryEventsContextBuilder.Build(context);
            if (settings.ConfigureStaticProviders)
                ServiceDiscoveryEventsContextProvider.Configure(context.ServiceDiscoveryEventsContext, true);
            context.ServiceBeacon = serviceBeaconBuilder.Build(context);

            if (settings.ConfigureStaticProviders)
                ClusterClientDefaults.ClientApplicationName = context.ServiceBeacon is ServiceBeacon beacon
                    ? beacon.ReplicaInfo.Application
                    : context.ApplicationIdentity.FormatServiceName();

            // todo (kungurtsev, 02.03.2023): if we use ActivitySourceTracer build it earlier
            context.SubstituteTracer(tracerBuilder.Build(context));

            if (settings.DiagnosticMetricsEnabled && diagnosticsBuilder.GetIntermediateBuilder(context).NeedsApplicationMetricsProvider)
            {
                context.MetricsInfoProvider = new ApplicationMetricsProvider();
                metricsBuilder.AddCustomization(metrics => metrics.AddMetricEventSender(context.MetricsInfoProvider));
            }

            context.Metrics = metricsBuilder.Build(context);
            if (settings.ConfigureStaticProviders)
                MetricContextProvider.Configure(context.Metrics.Root, true);

            if (settings.SendAnnotations)
                AnnotationsHelper.ReportLaunching(context.ApplicationIdentity, context.Metrics.Instance);

            if (settings.DiagnosticMetricsEnabled)
                context.RegisterDisposable(HerculesSinkMetrics.Measure(context.HerculesSink, context.Metrics, context.Log));

            if (settings.ConfigureStaticProviders)
                FlowingContext.Configuration.ErrorCallback = (errorMessage, error) => context.Log.ForContext(typeof(FlowingContext)).Error(error, errorMessage);

            context.ServiceBeacon.ReplicaInfo.TryGetUrl(out var url);

            var configSubstitutions = SubstitutionsProvider.Provide(
                    context.ApplicationIdentity,
                    context.ClusterConfigClient,
                    context.ServiceBeacon,
                    context.Datacenters)
                .ToArray();

            context.ConfigurationSource.SwitchTo(src => src.Substitute(configSubstitutions));
            context.SecretConfigurationSource.SwitchTo(src => src.Substitute(configSubstitutions));
            context.MergedConfigurationSource.SwitchTo(src => src.Substitute(configSubstitutions));

            context.DiagnosticsHub = settings.DiagnosticMetricsEnabled
                ? diagnosticsBuilder.Build(context)
                : new DiagnosticsHub(new DiagnosticInfo(), new HealthTracker(TimeSpan.MaxValue, context.Log));

            if (settings.SetupShutdownSupported)
                (context.HostingShutdown, context.ApplicationShutdown) = ShutdownFactory.Create(
                    context.ServiceBeacon,
                    context.ServiceLocator,
                    context.ApplicationIdentity,
                    context.Metrics.Instance,
                    context.Log,
                    url?.Port,
                    shutdownTokens,
                    shutdownTimeout,
                    settings.BeaconShutdownTimeout,
                    settings.BeaconShutdownWaitEnabled,
                    settings.SendAnnotations);

            var vostokHostingEnvironment = new VostokHostingEnvironment(
                context.HostingShutdown,
                context.ApplicationShutdown,
                context.ApplicationIdentity,
                context.ApplicationLimits,
                context.ApplicationReplication,
                context.Metrics,
                context.DiagnosticsHub,
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
                context.Datacenters,
                hostExtensionsBuilder.HostExtensions,
                context.Dispose);

            hostExtensionsBuilder.Build(context, vostokHostingEnvironment);

            if (settings.DiagnosticMetricsEnabled)
                systemMetricsBuilder.Build(context, vostokHostingEnvironment);

            if (!hasLogs)
            {
                context.LogDisabled("All logs");
                context.PrintBufferedLogs();
                context.Log = context.Logs.BuildCompositeLog(out _);
            }

            if (settings.DiagnosticMetricsEnabled)
                context.RegisterDisposable(LogLevelMetrics.Measure(context.Logs.LogEventLevelCounterFactory.CreateCounter(), context.Metrics));

            if (settings.ConfigureStaticProviders)
                StaticProvidersHelper.Configure(vostokHostingEnvironment);

            if (settings.DiagnosticMetricsEnabled)
                context.RegisterDisposable(HealthCheckMetrics.Measure(context.DiagnosticsHub.HealthTracker, context.Metrics));

            return vostokHostingEnvironment;
        }

        #region SetupComponents

        public IVostokHostingEnvironmentBuilder SetupShutdownToken(CancellationToken shutdownToken)
        {
            if (!settings.SetupShutdownSupported)
                throw new NotSupportedException("Setup shutdown token is not supported.");

            shutdownTokens.Add(shutdownToken);
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupShutdownTimeout(TimeSpan shutdownTimeout)
        {
            if (!settings.SetupShutdownSupported)
                throw new NotSupportedException("Setup shutdown token is not supported.");

            this.shutdownTimeout = shutdownTimeout.Cut(
                ShutdownConstants.CutAmountForExternalTimeout,
                ShutdownConstants.CutMaximumRelativeValue);

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

        public IVostokHostingEnvironmentBuilder SetupClusterConfigClient(Action<IVostokClusterConfigClientBuilder, IVostokConfigurationSetupContext> setup)
        {
            clusterConfigClientBuilder.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
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

        public IVostokHostingEnvironmentBuilder SetupDiagnostics(Action<IVostokDiagnosticsBuilder> setup)
        {
            diagnosticsBuilder.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupDiagnostics(Action<IVostokDiagnosticsBuilder, IVostokHostingEnvironmentSetupContext> setup)
        {
            diagnosticsBuilder.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
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
            zooKeeperClientBuilder.AddCustomization(b => b.AutoEnable());
            zooKeeperClientBuilder.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupZooKeeperClient(Action<IVostokZooKeeperClientBuilder, IVostokHostingEnvironmentSetupContext> setup)
        {
            zooKeeperClientBuilder.AddCustomization(b => b.AutoEnable());
            zooKeeperClientBuilder.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupHerculesSink(Action<IVostokHerculesSinkBuilder> setup)
        {
            herculesSinkBuilder.AddCustomization(b => b.AutoEnable());
            herculesSinkBuilder.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupHerculesSink(Action<IVostokHerculesSinkBuilder, IVostokHostingEnvironmentSetupContext> setup)
        {
            herculesSinkBuilder.AddCustomization(b => b.AutoEnable());
            herculesSinkBuilder.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupServiceBeacon(Action<IVostokServiceBeaconBuilder> setup)
        {
            serviceBeaconBuilder.AddCustomization(b => b.AutoEnable());
            serviceBeaconBuilder.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupServiceBeacon(Action<IVostokServiceBeaconBuilder, IVostokHostingEnvironmentSetupContext> setup)
        {
            serviceBeaconBuilder.AddCustomization(b => b.AutoEnable());
            serviceBeaconBuilder.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupServiceDiscoveryEventsContext(Action<IVostokServiceDiscoveryEventsContextBuilder> setup)
        {
            serviceDiscoveryEventsContextBuilder.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupServiceDiscoveryEventsContext(Action<IVostokServiceDiscoveryEventsContextBuilder, IVostokHostingEnvironmentSetupContext> setup)
        {
            serviceDiscoveryEventsContextBuilder.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
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

        public IVostokHostingEnvironmentBuilder SetupConfiguration(Action<IVostokConfigurationBuilder, IVostokConfigurationSetupContext> setup)
        {
            configurationBuilder.AddCustomization(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupSystemMetrics(Action<SystemMetricsSettings> setup)
        {
            systemMetricsBuilder.Customize(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        public IVostokHostingEnvironmentBuilder SetupSystemMetrics(Action<SystemMetricsSettings, IVostokHostingEnvironment> setup)
        {
            systemMetricsBuilder.Customize(setup ?? throw new ArgumentNullException(nameof(setup)));
            return this;
        }

        #endregion
    }
}