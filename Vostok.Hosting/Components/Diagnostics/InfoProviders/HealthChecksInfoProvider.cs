using Vostok.Hosting.Abstractions.Diagnostics;

namespace Vostok.Hosting.Components.Diagnostics.InfoProviders
{
    internal class HealthChecksInfoProvider : IDiagnosticInfoProvider
    {
        private readonly IHealthTracker healthTracker;

        public HealthChecksInfoProvider(IHealthTracker healthTracker)
            => this.healthTracker = healthTracker;

        public object Query() => healthTracker.CurrentReport;
    }
}
