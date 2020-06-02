using System;
using Vostok.Commons.Helpers;
using Vostok.Hosting.Setup;

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

            return new DiagnosticInfo();
        }

        private HealthTracker BuilderHealthTracker(BuildContext context)
        {
            var healthSettings = healthSettingsCustomization.Customize(new HealthTrackerSettings());
            
            return new HealthTracker(healthSettings.ChecksPeriod, context.Log);
        }
    }
}
