using System;
using JetBrains.Annotations;
using Vostok.Commons.Time;

namespace Vostok.Hosting.Components.Diagnostics
{
    [PublicAPI]
    public class HealthTrackerSettings
    {
        public TimeSpan ChecksPeriod { get; set; } = 30.Seconds();

        public bool AddDatacenterWhitelistCheck { get; set; } = true;

        public bool AddThreadPoolStarvationCheck { get; set; } = true;

        public bool AddZooKeeperConnectionCheck { get; set; } = true;

        public bool AddDnsResolutionCheck { get; set; } = true;

        public bool AddConfigurationCheck { get; set; } = true;
    }
}