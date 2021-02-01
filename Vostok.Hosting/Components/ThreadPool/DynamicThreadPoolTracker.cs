using System;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Commons.Helpers.Extensions;
using Vostok.Commons.Threading;
using Vostok.Configuration.Abstractions;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.Components.ThreadPool
{
    internal class DynamicThreadPoolTracker : IDisposable
    {
        private readonly TimeSpan checkPeriod;

        private readonly ILog log;
        private readonly IConfigurationProvider configProvider;
        private readonly IVostokApplicationLimits applicationLimits;
        private readonly Func<IConfigurationProvider, ThreadPoolSettings> settingsProvider;

        private readonly CancellationTokenSource cancellation;
        private volatile Task checkerTask;

        private volatile ThreadPoolSettings previousSettings;

        public DynamicThreadPoolTracker(
            TimeSpan checkPeriod,
            IConfigurationProvider configProvider,
            IVostokApplicationLimits limits,
            Func<IConfigurationProvider, ThreadPoolSettings> settingsProvider,
            ILog log)
        {
            this.log = log.ForContext<DynamicThreadPoolTracker>();
            this.configProvider = configProvider;
            applicationLimits = limits;
            this.settingsProvider = settingsProvider;

            this.checkPeriod = checkPeriod;

            cancellation = new CancellationTokenSource();
            previousSettings = new ThreadPoolSettings();
        }

        public void LaunchPeriodicalChecks(CancellationToken externalToken)
        {
            var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(externalToken, cancellation.Token);

            // ReSharper disable once MethodSupportsCancellation
            Interlocked.Exchange(ref checkerTask, Task.Run(() => RunPeriodicallyAsync(linkedCancellation.Token)));
        }

        public void Dispose()
        {
            cancellation.Cancel();

            checkerTask?.SilentlyContinue().GetAwaiter().GetResult();
        }

        private static bool WereSettingsUpdated(ThreadPoolSettings oldSettings, ThreadPoolSettings newSettings)
        {
            return oldSettings == null ||
                   newSettings.ThreadPoolMultiplier != oldSettings.ThreadPoolMultiplier ||
                   !newSettings.CpuUnits.Equals(oldSettings.CpuUnits);
        }

        private static void SetupThreadPool(ThreadPoolSettings settings)
        {
            if (settings.CpuUnits.HasValue)
                ThreadPoolUtility.Setup(settings.ThreadPoolMultiplier, settings.CpuUnits.Value);
            else
                ThreadPoolUtility.Setup(settings.ThreadPoolMultiplier);
        }

        private async Task RunPeriodicallyAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (TryGetThreadPoolSettings(out var currentSettings) && WereSettingsUpdated(previousSettings, currentSettings))
                {
                    SetupThreadPool(currentSettings);

                    previousSettings = currentSettings;

                    LogThreadPoolSettings(currentSettings);
                }

                await Task.Delay(checkPeriod, cancellationToken).ConfigureAwait(false);
            }
        }

        private bool TryGetThreadPoolSettings(out ThreadPoolSettings currentSettings)
        {
            try
            {
                currentSettings = settingsProvider(configProvider);
                
                // TODO: Also, create settings class and builder so it's possible to setup everything with builder.
                
                currentSettings.CpuUnits = applicationLimits.CpuUnits;

                return true;
            }
            catch (Exception error)
            {
                log.Warn(error, "Unable to update thread pool settings.");

                currentSettings = null;
                return false;
            }
        }

        private void LogThreadPoolSettings(ThreadPoolSettings settings)
        {
            // TODO: Proper logging?
            log.Info(
                "New thread pool multiplier: {multiplier}. New CPU units value: {units:D}",
                settings.ThreadPoolMultiplier,
                settings.CpuUnits);
        }
    }
}