using System;
using Vostok.Commons.Threading;
using Vostok.Commons.Time;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.Components.ThreadPool
{
    public class DynamicThreadPoolTracker : IDisposable
    {
        private static readonly TimeSpan ChecksPeriod = 10.Seconds();

        private readonly PeriodicalAction threadPoolUpdate;

        private readonly ILog log;
        private readonly IVostokApplicationLimits applicationLimits;
        private readonly Func<ThreadPoolSettings> settingsProvider;

        private int? previousThreadPoolMultiplier;
        private float? previousCpuUnits;

        public DynamicThreadPoolTracker(
            Func<ThreadPoolSettings> settingsProvider,
            IVostokApplicationLimits limits,
            ILog log)
        {
            this.log = log.ForContext<DynamicThreadPoolTracker>();
            applicationLimits = limits;
            this.settingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));

            threadPoolUpdate = new PeriodicalAction(CheckAndUpdate, LogOnException, () => ChecksPeriod);
            threadPoolUpdate.Start();
        }

        public void Dispose()
        {
            threadPoolUpdate.Stop();
        }

        private bool WereSettingsUpdated(int newThreadPoolMultiplier, float? newCpuUnits)
        {
            return previousThreadPoolMultiplier != newThreadPoolMultiplier ||
                   !previousCpuUnits.Equals(newCpuUnits);
        }

        private void CheckAndUpdate()
        {
            var newThreadPoolMultiplier = settingsProvider().ThreadPoolMultiplier;
            var newCpuUnits = applicationLimits.CpuUnits;

            if (WereSettingsUpdated(newThreadPoolMultiplier, newCpuUnits))
            {
                ThreadPoolUtility.Setup(newThreadPoolMultiplier, newCpuUnits);

                previousThreadPoolMultiplier = newThreadPoolMultiplier;
                previousCpuUnits = newCpuUnits;

                LogThreadPoolSettings(newThreadPoolMultiplier, newCpuUnits);
            }
        }

        private void LogThreadPoolSettings(int threadPoolMultiplier, float? cpuUnits)
        {
            log.Info(
                "New thread pool multiplier: {multiplier}. New CPU units value: {units}",
                threadPoolMultiplier,
                cpuUnits.HasValue ? $"{cpuUnits.Value:F2} core(s)" : "<unlimited>");
        }

        private void LogOnException(Exception error)
        {
            log.Warn(error, "Unable to update thread pool settings.");
        }
    }
}