using JetBrains.Annotations;

namespace Vostok.Hosting.Components.Log
{
    [PublicAPI]
    public class EventLevelCounterFactory
    {
        private readonly LevelCountingLog baseLog;

        internal EventLevelCounterFactory(LevelCountingLog baseLog)
        {
            this.baseLog = baseLog;
        }

        public EventLevelCounter CreateCounter()
        {
            var counter = new EventLevelCounter();
            baseLog.AddCounter(counter);
            return counter;
        }
    }
}