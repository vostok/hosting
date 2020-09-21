using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Commons.Helpers.Extensions;
using Vostok.Commons.Time;
using Vostok.Logging.Abstractions;
using Vostok.ServiceDiscovery.Abstractions;

// ReSharper disable MethodSupportsCancellation
// ReSharper disable PossiblyImpureMethodCallOnReadonlyVariable

namespace Vostok.Hosting.Components.Shutdown
{
    internal class HostingShutdown : IDisposable
    {
        private readonly ApplicationShutdown appShutdown;
        private readonly IServiceBeacon serviceBeacon;
        private readonly IServiceLocator serviceLocator;
        private readonly ILog log;

        private readonly CancellationTokenRegistration tokenRegistration;
        private readonly TimeSpan totalTimeout;
        private readonly TimeSpan beaconTimeout;
        private readonly bool beaconWaitEnabled;

        private readonly TimeBudget hostShutdownBudget;

        public HostingShutdown(
            ApplicationShutdown appShutdown,
            IServiceBeacon serviceBeacon,
            IServiceLocator serviceLocator,
            ILog log,
            CancellationToken token,
            TimeSpan totalTimeout,
            TimeSpan beaconTimeout,
            bool beaconWaitEnabled)
        {
            this.appShutdown = appShutdown;
            this.serviceBeacon = serviceBeacon;
            this.serviceLocator = serviceLocator;
            this.log = log.ForContext<HostingShutdown>();

            this.totalTimeout = totalTimeout;
            this.beaconTimeout = beaconTimeout;
            this.beaconWaitEnabled = beaconWaitEnabled;

            hostShutdownBudget = TimeBudget.CreateNew(totalTimeout);
            tokenRegistration = token.Register(OnHostShutdownTriggered);
        }

        public void Dispose()
            => tokenRegistration.Dispose();

        private void OnHostShutdownTriggered()
        {
            log.Info("Hosting shutdown has been initiated. Timeout = {HostingShutdownTimeout}.", totalTimeout);

            hostShutdownBudget.Start();

            ShutdownBeaconAsync().ContinueWith(_ => appShutdown.Initiate(hostShutdownBudget.Remaining));
        }

        private Task ShutdownBeaconAsync()
        {
            // (iloktionov): Task.Run() protects from blocking the caller of relevant CancellationTokenSource.Cancel().
            // (iloktionov): External timeout protects from serviceBeacon.Stop() call taking too long.
            // (iloktionov): Artifical timeout reduction protects solely from false warning logs.
            return Task.Run(() => ShutdownBeaconAsync(beaconTimeout.Cut(ShutdownConstants.CutAmountForBeaconTimeout, ShutdownConstants.CutMaximumRelativeValue)))
                .WaitAsync(beaconTimeout)
                .ContinueWith(
                    task =>
                    {
                        if (!task.Result && beaconTimeout > TimeSpan.Zero)
                            log.Warn("Failed to shut down service beacon in {ServiceBeaconShutdownTimeout}.", beaconTimeout);
                    });
        }

        private Task ShutdownBeaconAsync(TimeSpan timeout)
        {
            var budget = TimeBudget.StartNew(timeout);

            // (iloktionov): No reason to wait for deregistration if serviceBeacon.Stop() call has failed.
            if (!StopServiceBeacon(budget))
                return Task.CompletedTask;

            return beaconWaitEnabled ? WaitForBeaconShutdownAsync(budget) : Task.CompletedTask;
        }

        private bool StopServiceBeacon(TimeBudget budget)
        {
            var elapsedBefore = budget.Elapsed;

            log.Info("Stopping service beacon..");

            try
            {
                serviceBeacon.Stop();

                log.Info("Stopped service beacon in {ServiceBeaconStopTime}.", budget.Elapsed - elapsedBefore);

                return true;
            }
            catch (Exception error)
            {
                log.Error(error, "Failed to stop service beacon.");

                return false;
            }
        }

        private async Task WaitForBeaconShutdownAsync(TimeBudget budget)
        {
            try
            {
                log.Info("Service beacon graceful deregistration has been initiated (up to {ServiceBeaconWaitTime}).", budget.Remaining);

                var replicaInfo = serviceBeacon.ReplicaInfo;
                var elapsedBefore = budget.Elapsed;

                while (!budget.HasExpired)
                {
                    var topology = serviceLocator.Locate(replicaInfo.Environment, replicaInfo.Application);

                    var replica = topology?.Replicas.FirstOrDefault(r => r.ToString().Equals(replicaInfo.Replica, StringComparison.OrdinalIgnoreCase));
                    if (replica == null)
                    {
                        log.Info("Service replica has disappeared from topology according to local service locator in {ServiceBeaconWaitDuration}.", budget.Elapsed - elapsedBefore);
                        break;
                    }

                    await Task.Delay(TimeSpanArithmetics.Min(budget.Remaining, 100.Milliseconds())).ConfigureAwait(false);
                }

                // (iloktionov): The rest of the wait is a safety net (other applications may receive SD notifications significantly later).
                await Task.Delay(budget.Remaining).ConfigureAwait(false);
            }
            catch (Exception error)
            {
                log.Error(error, "Failed to wait gracefully for service beacon deregistration.");
            }
        }
    }
}
