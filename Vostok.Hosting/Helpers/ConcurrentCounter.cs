using System.Threading;

namespace Vostok.Hosting.Helpers
{
    internal class ConcurrentCounter
    {
        private volatile int value;

        public void Increment() => Interlocked.Increment(ref value);

        public int CollectAndReset() => Interlocked.Exchange(ref value, 0);
    }
}