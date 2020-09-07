using System.Collections.Generic;
using JetBrains.Annotations;
using System.Collections.Immutable;
using System.Threading;

namespace Vostok.Hosting.Components.Log
{
    [PublicAPI]
    public class EventLevelCounterFactory
    {
        private volatile ImmutableList<EventLevelCounter> counters = ImmutableList<EventLevelCounter>.Empty;

        public EventLevelCounter CreateCounter()
        {
            var counter = new EventLevelCounter();
            var newList = counters.Add(counter);
            Interlocked.Exchange(ref counters, newList);
            return counter;
        }

        internal IEnumerable<EventLevelCounter> GetCounters()
        {
            return counters;
        }
    }
}