using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Vostok.Commons.Time;
using Vostok.Hosting.Helpers;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.Components.Log
{
    [PublicAPI]
    public class EventLevelCounter
    {
        private static readonly TimeSpan CacheTtl = 1.Minutes();

        private readonly ConcurrentCounter debugEvents = new ConcurrentCounter();
        private readonly ConcurrentCounter infoEvents = new ConcurrentCounter();
        private readonly ConcurrentCounter warnEvents = new ConcurrentCounter();
        private readonly ConcurrentCounter errorEvents = new ConcurrentCounter();
        private readonly ConcurrentCounter fatalEvents = new ConcurrentCounter();
        private readonly Stopwatch stopwatch = Stopwatch.StartNew();

        private readonly TimeCache<LogEventsMetrics> cachedValue;

        public EventLevelCounter()
        {
            cachedValue = new TimeCache<LogEventsMetrics>(CollectInner, CacheTtl);
        }

        internal void HandleEvent(LogEvent @event)
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

        public LogEventsMetrics Collect() => cachedValue.GetValue();

        private LogEventsMetrics CollectInner()
        {
            var deltaTime = stopwatch.Elapsed.TotalMinutes;

            var result = new LogEventsMetrics(
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