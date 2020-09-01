using JetBrains.Annotations;
using Vostok.Hosting.Helpers;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.Components.Log
{
    [PublicAPI]
    public class EventLevelCounter
    {
        private readonly ConcurrentCounter debugEvents = new ConcurrentCounter();
        private readonly ConcurrentCounter infoEvents = new ConcurrentCounter();
        private readonly ConcurrentCounter warnEvents = new ConcurrentCounter();
        private readonly ConcurrentCounter errorEvents = new ConcurrentCounter();
        private readonly ConcurrentCounter fatalEvents = new ConcurrentCounter();

        public LogEventsMetrics Collect() =>
            new LogEventsMetrics(
                debugEvents.CollectAndReset(),
                infoEvents.CollectAndReset(),
                warnEvents.CollectAndReset(),
                errorEvents.CollectAndReset(),
                fatalEvents.CollectAndReset()
            );

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
    }
}