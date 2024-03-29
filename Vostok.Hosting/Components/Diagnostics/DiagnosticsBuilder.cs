﻿using System;
using Vostok.Commons.Environment;
using Vostok.Commons.Helpers;
using Vostok.Hercules.Client;
using Vostok.Hosting.Abstractions.Diagnostics;
using Vostok.Hosting.Components.Diagnostics.HealthChecks;
using Vostok.Hosting.Components.Diagnostics.InfoProviders;
using Vostok.Hosting.Setup;
using Vostok.ZooKeeper.Client;

namespace Vostok.Hosting.Components.Diagnostics
{
    internal class DiagnosticsBuilder : IVostokDiagnosticsBuilder, IBuilder<DiagnosticsHub>
    {
        private readonly Customization<HealthTrackerSettings> healthSettingsCustomization;
        private readonly Customization<DiagnosticInfoSettings> infoSettingsCustomization;

        public DiagnosticsBuilder()
        {
            healthSettingsCustomization = new Customization<HealthTrackerSettings>();
            infoSettingsCustomization = new Customization<DiagnosticInfoSettings>();
        }

        public bool NeedsApplicationMetricsProvider
            => infoSettingsCustomization.Customize(new DiagnosticInfoSettings()).AddApplicationMetricsInfo;

        public IVostokDiagnosticsBuilder CustomizeInfo(Action<DiagnosticInfoSettings> customization)
        {
            infoSettingsCustomization.AddCustomization(customization ?? throw new ArgumentNullException(nameof(customization)));
            return this;
        }

        public IVostokDiagnosticsBuilder CustomizeHealthTracker(Action<HealthTrackerSettings> customization)
        {
            healthSettingsCustomization.AddCustomization(customization ?? throw new ArgumentNullException(nameof(customization)));
            return this;
        }

        public DiagnosticsHub Build(BuildContext context)
        {
            var healthTracker = BuildHealthTracker(context);
            var diagnosticInfo = BuildDiagnosticInfo(context, healthTracker);

            return new DiagnosticsHub(diagnosticInfo, healthTracker);
        }

        private HealthTracker BuildHealthTracker(BuildContext context)
        {
            var healthSettings = healthSettingsCustomization.Customize(new HealthTrackerSettings());
            var healthTracker = new HealthTracker(healthSettings.ChecksPeriod, context.Log);

            if (healthSettings.AddDatacenterWhitelistCheck)
                healthTracker.RegisterCheck(WellKnownHealthCheckNames.DatacenterWhitelist, new DatacenterWhitelistCheck(context.Datacenters));

            if (healthSettings.AddThreadPoolStarvationCheck)
                healthTracker.RegisterCheck(WellKnownHealthCheckNames.ThreadPoolStarvation, new ThreadPoolStarvationCheck());

            if (healthSettings.AddZooKeeperConnectionCheck && context.ZooKeeperClient is ZooKeeperClient realClient)
                healthTracker.RegisterCheck(WellKnownHealthCheckNames.ZooKeeperConnection, new ZooKeeperConnectionCheck(realClient));

            if (healthSettings.AddDnsResolutionCheck && RuntimeDetector.IsDotNet50AndNewer)
                healthTracker.RegisterCheck(WellKnownHealthCheckNames.DnsResolution, context.RegisterDisposable(new DnsResolutionCheck()));

            if (healthSettings.AddConfigurationCheck)
            {
                healthTracker.RegisterCheck(WellKnownHealthCheckNames.Configuration, context.RegisterDisposable(new ConfigurationCheck(context.ConfigurationProvider)));
                healthTracker.RegisterCheck(WellKnownHealthCheckNames.SecretConfiguration, context.RegisterDisposable(new ConfigurationCheck(context.SecretConfigurationProvider)));
            }

            return healthTracker;
        }

        private DiagnosticInfo BuildDiagnosticInfo(BuildContext context, IHealthTracker healthTracker)
        {
            var infoSettings = infoSettingsCustomization.Customize(new DiagnosticInfoSettings());
            var info = new DiagnosticInfo();

            if (infoSettings.AddEnvironmentInfo)
                info.RegisterProvider(CreateEntry(WellKnownDiagnosticInfoProvidersNames.EnvironmentInfo), new EnvironmentInfoProvider(context.Datacenters));

            if (infoSettings.AddSystemMetricsInfo)
                info.RegisterProvider(CreateEntry(WellKnownDiagnosticInfoProvidersNames.SystemMetrics), context.RegisterDisposable(new SystemMetricsProvider()));

            if (infoSettings.AddLoadedAssembliesInfo)
                info.RegisterProvider(CreateEntry(WellKnownDiagnosticInfoProvidersNames.LoadedAssemblies), new LoadedAssembliesProvider());

            if (infoSettings.AddHealthChecksInfo)
                info.RegisterProvider(CreateEntry(WellKnownDiagnosticInfoProvidersNames.HealthChecks), new HealthChecksInfoProvider(healthTracker));

            if (infoSettings.AddConfigurationInfo)
                info.RegisterProvider(CreateEntry(WellKnownDiagnosticInfoProvidersNames.Configuration), new ConfigurationInfoProvider(context.ConfigurationSource));

            if (infoSettings.AddHerculesSinkInfo && context.HerculesSink is HerculesSink realSink)
                info.RegisterProvider(CreateEntry(WellKnownDiagnosticInfoProvidersNames.HerculesSink), new HerculesSinkInfoProvider(realSink));

            if (infoSettings.AddApplicationMetricsInfo && context.MetricsInfoProvider != null)
                info.RegisterProvider(CreateEntry(WellKnownDiagnosticInfoProvidersNames.ApplicationMetrics), context.MetricsInfoProvider);

            if (infoSettings.AddApplicationInfo)
                info.RegisterProvider(
                    CreateEntry(WellKnownDiagnosticInfoProvidersNames.ApplicationInfo),
                    new ApplicationInfoProvider(
                        context.ApplicationIdentity,
                        context.ApplicationLimits,
                        context.ApplicationReplication));

            return info;
        }

        private static DiagnosticEntry CreateEntry(string name)
            => new DiagnosticEntry(WellKnownDiagnosticComponentsNames.Hosting, name);
    }
}