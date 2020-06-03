using System;
using Vostok.Commons.Helpers;
using Vostok.Hosting.Abstractions.Diagnostics;
using Vostok.Hosting.Components.Diagnostics.HealthChecks;
using Vostok.Hosting.Components.Diagnostics.InfoProviders;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.Components.Diagnostics
{
    internal class DiagnosticsBuilder : IVostokDiagnosticsBuilder, IBuilder<DiagnosticsHub>
    {
        private const string DiagnosticInfoComponent = "hosting";

        private readonly Customization<HealthTrackerSettings> healthSettingsCustomization;
        private readonly Customization<DiagnosticInfoSettings> infoSettingsCustomization;

        public DiagnosticsBuilder()
        {
            healthSettingsCustomization = new Customization<HealthTrackerSettings>();
            infoSettingsCustomization = new Customization<DiagnosticInfoSettings>();
        }

        public IVostokDiagnosticsBuilder CustomizeInfo(Action<DiagnosticInfoSettings> customization)
        {
            infoSettingsCustomization.AddCustomization(customization ?? throw new ArgumentNullException(nameof(customization)));
            return this;
        }

        public IVostokDiagnosticsBuilder CustomizeHealth(Action<HealthTrackerSettings> customization)
        {
            healthSettingsCustomization.AddCustomization(customization ?? throw new ArgumentNullException(nameof(customization)));
            return this;
        }

        public DiagnosticsHub Build(BuildContext context)
            => new DiagnosticsHub(BuildDiagnosticInfo(context), BuilderHealthTracker(context));

        private DiagnosticInfo BuildDiagnosticInfo(BuildContext context)
        {
            var infoSettings = infoSettingsCustomization.Customize(new DiagnosticInfoSettings());
            var info = new DiagnosticInfo();

            if (infoSettings.AddEnvironmentInfo)
                info.RegisterProvider(CreateEntry("environment-info"), new EnvironmentInfoProvider());

            if (infoSettings.AddSystemMetricsInfo)
                info.RegisterProvider(CreateEntry("system-metrics"), new SystemMetricsProvider());

            if (infoSettings.AddLoadedAssembliesInfo)
                info.RegisterProvider(CreateEntry("loaded-assemblies"), new LoadedAssembliesProvider());

            return info;
        }

        private HealthTracker BuilderHealthTracker(BuildContext context)
        {
            var healthSettings = healthSettingsCustomization.Customize(new HealthTrackerSettings());
            var healthTracker = new HealthTracker(healthSettings.ChecksPeriod, context.Log);

            if (healthSettings.AddDatacenterWhitelistCheck)
                healthTracker.RegisterCheck("datacenter whitelist", new DatacenterWhitelistCheck(context.Datacenters));

            if (healthSettings.AddThreadPoolStartvationCheck)
                healthTracker.RegisterCheck("thread pool", new ThreadPoolStarvationCheck());

            return healthTracker;
        }

        private static DiagnosticEntry CreateEntry(string name)
            => new DiagnosticEntry(DiagnosticInfoComponent, name);
    }
}
