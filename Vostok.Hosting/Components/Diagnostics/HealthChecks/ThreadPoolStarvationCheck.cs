using System.Threading;
using System.Threading.Tasks;
using Vostok.Commons.Threading;
using Vostok.Hosting.Abstractions.Diagnostics;

namespace Vostok.Hosting.Components.Diagnostics.HealthChecks
{
    internal class ThreadPoolStarvationCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckAsync(CancellationToken cancellationToken)
        {
            var state = ThreadPoolUtility.GetPoolState();

            if (state.UsedWorkerThreads >= state.MinWorkerThreads)
                return Task.FromResult(HealthCheckResult.Degraded($"Worker threads in thread pool are exhaused: {state.UsedWorkerThreads}/{state.MinWorkerThreads} (used/min)."));

            if (state.UsedIocpThreads >= state.MinIocpThreads)
                return Task.FromResult(HealthCheckResult.Degraded($"IOCP threads in thread pool are exhaused: {state.UsedIocpThreads}/{state.MinIocpThreads} (used/min)."));

            return Task.FromResult(HealthCheckResult.Healthy());
        }
    }
}
