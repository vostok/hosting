using System;
using Vostok.Commons.Time;
using Vostok.Hosting.Abstractions.Diagnostics;

namespace Vostok.Hosting.Components.Diagnostics.InfoProviders
{
    internal class HealthChecksInfoProvider : IDiagnosticInfoProvider
    {
        private readonly IHealthTracker healthTracker;

        public HealthChecksInfoProvider(IHealthTracker healthTracker)
            => this.healthTracker = healthTracker;

        public object Query()
        {
            var report = healthTracker.CurrentReport;

            return new
            {
                CurrentStatus = report.Status,
                Duration = report.Duration.ToPrettyString(),
                Timestamp = report.Timestamp.ToString("O"),
                Recency = (DateTime.UtcNow - report.Timestamp).ToPrettyString() + " ago",
                report.Problems,
                report.Checks
            };
        }
    }
}
