using System;
using System.Diagnostics;
using Vostok.Commons.Time;
using Vostok.Hosting.Helpers;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.Components.Log
{
    internal class EventLevelCounter
    {
        private static readonly TimeSpan CacheTtl = 1.Minutes();

        private readonly ConcurrentCounter debugEvents = new ConcurrentCounter();
        private readonly ConcurrentCounter infoEvents = new ConcurrentCounter();
        private readonly ConcurrentCounter warnEvents = new ConcurrentCounter();
        private readonly ConcurrentCounter errorEvents = new ConcurrentCounter();
        private readonly ConcurrentCounter fatalEvents = new ConcurrentCounter();
        private readonly Stopwatch stopwatch = Stopwatch.StartNew();

        private readonly TimeCache<LogLevelStatistics> cachedValue;

        public EventLevelCounter()
        {
            cachedValue = new TimeCache<LogLevelStatistics>(CollectInner, CacheTtl);
        }

        public void HandleEvent(LogEvent @event)
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (@event?.Level)
            {
                case LogLevel.Debug:
                    debugEvents.Increment();
                    break;
                case LogLevel.Info:
                    infoEvents.Increment();
                    break;
                case LogLevel.Warn:
                    warnEvents.Increment();
                    break;
                case LogLevel.Error:
                    errorEvents.Increment();
                    break;
                case LogLevel.Fatal:
                    fatalEvents.Increment();
                    break;
            }
        }

        public LogLevelStatistics CollectStatistics() => cachedValue.GetValue();

        private LogLevelStatistics CollectInner()
        {
            var deltaTime = stopwatch.Elapsed.TotalMinutes;
            var result = new LogLevelStatistics(
                (int)(debugEvents.CollectAndReset() / deltaTime),
                (int)(infoEvents.CollectAndReset() / deltaTime),
                (int)(warnEvents.CollectAndReset() / deltaTime),
                (int)(errorEvents.CollectAndReset() / deltaTime),
                (int)(fatalEvents.CollectAndReset() / deltaTime));
            stopwatch.Restart();
            return result;
        }
    }
}