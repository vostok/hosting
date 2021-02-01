using System;
using JetBrains.Annotations;
using Vostok.Commons.Time;

namespace Vostok.Hosting.Components.SystemMetrics
{
    [PublicAPI]
    public class SystemMetricsSettings
    {
        public bool EnableGcEventsLogging { get; set; } = true;

        public bool EnableGcEventsMetrics { get; set; } = true;

        public bool EnableProcessMetricsLogging { get; set; } = true;

        public bool EnableProcessMetricsReporting { get; set; } = true;

        public bool EnableHostMetricsLogging { get; set; }

        public bool EnableHostMetricsReporting { get; set; }

        public TimeSpan GcMinimumDurationForLogging { get; set; } = 500.Milliseconds();

        public TimeSpan ProcessMetricsLoggingPeriod { get; set; } = 10.Seconds();
        
        public TimeSpan? ProcessMetricsReportingPeriod { get; set; }

        public TimeSpan HostMetricsLoggingPeriod { get; set; } = 10.Seconds();

        public TimeSpan? HostMetricsReportingPeriod { get; set; }
    }
}
