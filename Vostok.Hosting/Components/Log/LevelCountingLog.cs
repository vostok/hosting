using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.Components.Log
{
    internal class LevelCountingLog : ILog
    {
        private readonly EventLevelCounter eventLevelCounter;
        private volatile ILog baseLog;

        public LevelCountingLog(ILog baseLog, EventLevelCounter parentCounter)
        {
            this.baseLog = baseLog;
            eventLevelCounter = parentCounter;
        }

        public void Log(LogEvent @event)
        {
            eventLevelCounter.HandleEvent(@event);

            baseLog.Log(@event);
        }

        public bool IsEnabledFor(LogLevel level) => baseLog.IsEnabledFor(level);

        public ILog ForContext(string context) => new LevelCountingLog(baseLog.ForContext(context), eventLevelCounter);
    }
}