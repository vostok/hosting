using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Hosting.Abstractions.Diagnostics;
using Vostok.Metrics.System.Process;

namespace Vostok.Hosting.Components.Diagnostics.HealthChecks
{
    [PublicAPI]
    public class DnsResolutionCheck : IHealthCheck, IObserver<CurrentProcessMetrics>, IDisposable
    {
        private const double FailedLookupsThreshold = 0.2;

        private static readonly TimeSpan Period = TimeSpan.FromMinutes(5);

        private readonly CurrentProcessMonitor monitor = new CurrentProcessMonitor();
        private readonly IDisposable subscription;

        private DnsStatistics dnsStatistics;

        public DnsResolutionCheck() =>
            subscription = monitor.ObserveMetrics(Period).Subscribe(this);

        public Task<HealthCheckResult> CheckAsync(CancellationToken cancellationToken)
        {
            if (dnsStatistics.FailedLookupsFraction > FailedLookupsThreshold)
                return Task.FromResult(HealthCheckResult.Degraded($"Percentage of failed DNS lookups exceeded: {dnsStatistics.FailedLookupsFraction * 100} %"));

            return Task.FromResult(HealthCheckResult.Healthy());
        }

        public void OnNext(CurrentProcessMetrics value)
        {
            dnsStatistics = new DnsStatistics(value.FailedDnsLookupsCount ?? 0, value.DnsLookupsCount ?? 0);
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void Dispose()
        {
            subscription?.Dispose();
            monitor?.Dispose();
        }

        private struct DnsStatistics
        {
            private readonly int failedLookupsCount;
            private readonly int lookupsCount;

            public DnsStatistics(int failedLookupsCount, int lookupsCount)
            {
                this.failedLookupsCount = failedLookupsCount;
                this.lookupsCount = lookupsCount;
            }

            public double FailedLookupsFraction => lookupsCount != 0 ? (double)failedLookupsCount / lookupsCount : 0;
        }
    }
}