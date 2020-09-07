using System.Collections.Generic;
using JetBrains.Annotations;

namespace Vostok.Hosting.Components.Log
{
    [PublicAPI]
    public class EventLevelCounterFactory
    {
        private readonly List<EventLevelCounter> counters = new List<EventLevelCounter>();

        public EventLevelCounter CreateCounter()
        {
            var counter = new EventLevelCounter();
            counters.Add(counter);
            return counter;
        }

        internal IEnumerable<EventLevelCounter> GetCounters()
        {
            return counters;
        }
    }
}