using System;
using System.Diagnostics;
using System.Threading;

namespace Vostok.Hosting.Helpers
{
    internal class TimeCache<T>
        where T : class
    {
        private readonly Func<T> provider;
        private readonly TimeSpan ttl;
        private readonly Stopwatch stopwatch = new Stopwatch();

        private volatile T cachedValue;

        public TimeCache(
            Func<T> provider,
            TimeSpan ttl)
        {
            this.provider = provider;
            this.ttl = ttl;
        }

        public T GetValue()
        {
            if (!stopwatch.IsRunning || stopwatch.Elapsed >= ttl)
            {
                var newValue = provider();
                Interlocked.Exchange(ref cachedValue, newValue);
                stopwatch.Restart();
            }

            return cachedValue;
        }
    }
}