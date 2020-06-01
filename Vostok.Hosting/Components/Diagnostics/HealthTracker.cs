using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Commons.Helpers.Disposable;
using Vostok.Commons.Helpers.Observable;
using Vostok.Hosting.Abstractions.Diagnostics;

namespace Vostok.Hosting.Components.Diagnostics
{
    internal class HealthTracker : IHealthTracker
    {
        private readonly CachingObservable<HealthStatus> statusObservable;
        private readonly CachingObservable<(HealthStatus previous, HealthStatus current)> statusChangesObservable;
        private readonly ConcurrentDictionary<string, IHealthCheck> checks;

        private volatile HealthReport currentReport;

        public HealthTracker()
        {
            currentReport = new HealthReport(HealthStatus.Healthy, new Dictionary<string, HealthCheckResult>());
            statusObservable = new CachingObservable<HealthStatus>(currentReport.Status);
            statusChangesObservable = new CachingObservable<(HealthStatus previous, HealthStatus current)>();
            checks = new ConcurrentDictionary<string, IHealthCheck>(StringComparer.OrdinalIgnoreCase);
        }

        public HealthStatus CurrentStatus => currentReport.Status;

        public IObservable<HealthStatus> ObserveStatus() => statusObservable;

        public IObservable<(HealthStatus previous, HealthStatus current)> ObserveStatusChanges() => statusChangesObservable;

        public IDisposable RegisterCheck(string name, IHealthCheck check)
        {
            if (!checks.TryAdd(name, check))
                throw new InvalidOperationException($"Health check with name '{name}' is already registered.");

            return new ActionDisposable(() => checks.TryRemove(name, out _));
        }

        public Task<HealthReport> ObtainReportAsync(bool forceChecks) =>
            throw new NotImplementedException();

        // TODO(iloktionov): launch the checking loop

        private void HandleReport([NotNull] HealthReport report)
        {
            var oldReport = Interlocked.Exchange(ref currentReport, report);

            statusObservable.Next(report.Status);
            
            if (oldReport.Status != report.Status)
                statusChangesObservable.Next((oldReport.Status, report.Status));
        }
    }
}
