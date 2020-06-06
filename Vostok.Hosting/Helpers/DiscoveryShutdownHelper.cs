using System;
using System.Linq;
using System.Threading.Tasks;
using Vostok.Commons.Helpers.Extensions;
using Vostok.Commons.Time;
using Vostok.Hosting.Models;
using Vostok.Logging.Abstractions;
using Vostok.ServiceDiscovery;

namespace Vostok.Hosting.Helpers
{
    internal class DiscoveryShutdownHelper
    {
        private readonly VostokHostingEnvironment environment;
        private readonly ILog log;
        private readonly TimeSpan maxTimeout;

        public DiscoveryShutdownHelper(VostokHostingEnvironment environment, ILog log, TimeSpan maxTimeout)
        {
            this.environment = environment;
            this.log = log;
            this.maxTimeout = maxTimeout;
        }

        public Task WaitForGracefulShutdown()
        {
            if (environment.Port == null)
                return Task.CompletedTask;

            if (!(environment.ServiceBeacon is ServiceBeacon))
                return Task.CompletedTask;

            if (!(environment.ServiceLocator is ServiceLocator))
                return Task.CompletedTask;

            var timeout = TimeSpanArithmetics.Min(maxTimeout, environment.ShutdownTimeBudget.Remaining.Divide(3));

            log.Info("Service discovery graceful deregistration has been initiated (up to {ShutdownTimeout}).", timeout.ToPrettyString());

            return WaitForGracefulShutdownInternal(timeout).SilentlyContinue().WaitAsync(timeout);
        }

        private async Task WaitForGracefulShutdownInternal(TimeSpan timeout)
        {
            try
            {
                var budget = TimeBudget.StartNew(timeout);

                var replicaInfo = environment.ServiceBeacon.ReplicaInfo;

                while (!budget.HasExpired)
                {
                    var topology = environment.ServiceLocator.Locate(replicaInfo.Environment, replicaInfo.Application);

                    var replica = topology?.Replicas.FirstOrDefault(r => r.ToString().Equals(replicaInfo.Replica, StringComparison.OrdinalIgnoreCase));
                    if (replica == null)
                    {
                        log.Info("Service replica has disappeared from topology according to local service locator in {ShutdownDuration}.", budget.Elapsed.ToPrettyString());
                        break;
                    }

                    await Task.Delay(TimeSpanArithmetics.Min(budget.Remaining, 100.Milliseconds()));
                }

                // (iloktionov): The rest of the wait is a safety net (other applications may receive SD notifications significantly later).
                await Task.Delay(budget.Remaining).ConfigureAwait(false);
            }
            catch (Exception error)
            {
                log.Error(error);
            }
        }
    }
}
