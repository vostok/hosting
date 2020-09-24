using System.Collections.Generic;
using JetBrains.Annotations;

namespace Vostok.Hosting.Components.Log
{
    [PublicAPI]
    public class LogEventLevelCounterFactory
    {
        private readonly object counterLock = new object();
        private volatile List<LogEventLevelCounter> counters = new List<LogEventLevelCounter>();

        public LogEventLevelCounter CreateCounter()
        {
            var counter = new LogEventLevelCounter();
            var newList = new List<LogEventLevelCounter>();

            lock (counterLock)
            {
                newList.AddRange(counters);
                newList.Add(counter);
                counters = newList;
            }

            return counter;
        }

        internal IEnumerable<LogEventLevelCounter> GetCounters() => counters;
    }
}