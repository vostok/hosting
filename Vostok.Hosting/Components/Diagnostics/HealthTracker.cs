using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Commons.Helpers.Disposable;
using Vostok.Commons.Helpers.Extensions;
using Vostok.Commons.Helpers.Observable;
using Vostok.Commons.Time;
using Vostok.Hosting.Abstractions.Diagnostics;
using Vostok.Logging.Abstractions;

// ReSharper disable UseNullPropagation
// ReSharper disable MethodSupportsCancellation

namespace Vostok.Hosting.Components.Diagnostics
{
    internal class HealthTracker : IHealthTracker, IDisposable
    {
        private readonly TimeSpan checkPeriod;
        private readonly ILog log;

        private readonly CachingObservable<HealthStatus> statusObservable;
        private readonly CachingObservable<HealthReport> reportsObservable;
        private readonly CachingObservable<(HealthStatus previous, HealthStatus current)> statusChangesObservable;
        private readonly ConcurrentDictionary<string, IHealthCheck> checks;
        private readonly CancellationTokenSource cancellation;

        private volatile HealthReport currentReport;
        private volatile Task checkerTask;

        public HealthTracker(TimeSpan checkPeriod, ILog log)
        {
            this.checkPeriod = checkPeriod;
            this.log = log.ForContext<HealthTracker>();

            currentReport = new HealthReport(HealthStatus.Healthy, TimeSpan.Zero, DateTimeOffset.UtcNow, new Dictionary<string, HealthCheckResult>());
            statusObservable = new CachingObservable<HealthStatus>(currentReport.Status);
            reportsObservable = new CachingObservable<HealthReport>(currentReport);
            statusChangesObservable = new CachingObservable<(HealthStatus previous, HealthStatus current)>();
            checks = new ConcurrentDictionary<string, IHealthCheck>(StringComparer.OrdinalIgnoreCase);
            cancellation = new CancellationTokenSource();
        }

        public HealthStatus CurrentStatus => currentReport.Status;

        public HealthReport CurrentReport => currentReport;

        public IObservable<HealthStatus> ObserveStatus() => statusObservable;

        public IObservable<HealthReport> ObserveReports() => reportsObservable;

        public IObservable<(HealthStatus previous, HealthStatus current)> ObserveStatusChanges() => statusChangesObservable;

        public IDisposable RegisterCheck(string name, IHealthCheck check)
        {
            if (!checks.TryAdd(name, check))
                throw new InvalidOperationException($"Health check with name '{name}' is already registered.");

            return new ActionDisposable(() => checks.TryRemove(name, out _));
        }

        public IEnumerator<(string name, IHealthCheck check)> GetEnumerator() =>
            checks.Select(pair => (pair.Key, pair.Value)).GetEnumerator();

        public void Dispose()
        {
            cancellation.Cancel();

            if (checkerTask != null)
                checkerTask.SilentlyContinue().GetAwaiter().GetResult();
        }

        public void LaunchPeriodicalChecks(CancellationToken externalToken)
        {
            var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(externalToken, cancellation.Token);

            Interlocked.Exchange(ref checkerTask, Task.Run(() => RunPeriodicallyAsync(linkedCancellation.Token)));
        }

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        private async Task RunPeriodicallyAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var budget = TimeBudget.StartNew(checkPeriod);

                var report = await RunChecksAsync(cancellationToken).ConfigureAwait(false);

                HandleReport(report);

                await Task.Delay(budget.Remaining, cancellationToken);
            }
        }

        private async Task<HealthReport> RunChecksAsync(CancellationToken cancellationToken)
        {
            var results = new Dictionary<string, HealthCheckResult>(StringComparer.OrdinalIgnoreCase);

            var watch = Stopwatch.StartNew();

            foreach (var pair in checks)
            {
                cancellationToken.ThrowIfCancellationRequested();

                results[pair.Key] = await pair.Value.CheckSafeAsync(cancellationToken).ConfigureAwait(false);
            }

            var aggregateStatus = SelectStatus(results.Values);

            return new HealthReport(aggregateStatus, watch.Elapsed, DateTimeOffset.UtcNow, results);
        }

        private HealthStatus SelectStatus(IEnumerable<HealthCheckResult> results)
        {
            if (results.Any(r => r.Status == HealthStatus.Failing))
                return HealthStatus.Failing;

            if (results.Any(r => r.Status == HealthStatus.Degraded))
                return HealthStatus.Degraded;

            return HealthStatus.Healthy;
        }

        private void HandleReport(HealthReport report)
        {
            LogFailedHealthChecks(report);

            var oldReport = Interlocked.Exchange(ref currentReport, report);

            reportsObservable.Next(report);

            statusObservable.Next(report.Status);

            if (oldReport.Status != report.Status)
            {
                statusChangesObservable.Next((oldReport.Status, report.Status));
                LogChangedHealthStatus(oldReport.Status, report.Status);
            }
        }

        private void LogFailedHealthChecks(HealthReport report)
        {
            foreach (var pair in report.Checks)
            {
                if (pair.Value.Status == HealthStatus.Healthy)
                    continue;

                log.Warn(
                    "Health check '{HealthCheckName}' yielded '{HealthStatus}' result. Reason: {HealthStatusReason}",
                    pair.Key,
                    pair.Value.Status,
                    pair.Value.Reason);
            }
        }

        private void LogChangedHealthStatus(HealthStatus oldStatus, HealthStatus newStatus)
        {
            const string message = "Current health status changed from {OldHealthStatus} to {NewHealthStatus}.";

            if (newStatus == HealthStatus.Healthy)
                log.Info(message, oldStatus, newStatus);
            else
                log.Warn(message, oldStatus, newStatus);
        }
    }
}
