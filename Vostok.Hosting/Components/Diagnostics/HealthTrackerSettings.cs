using System;
using JetBrains.Annotations;
using Vostok.Commons.Time;

namespace Vostok.Hosting.Components.Diagnostics
{
    [PublicAPI]
    public class HealthTrackerSettings
    {
        public TimeSpan ChecksPeriod { get; set; } = 30.Seconds();
    }
}
