using JetBrains.Annotations;

namespace Vostok.Hosting.Components.ThreadPool
{
    /// <summary>
    /// Settings class that is used to dynamically configure thread pool.
    /// </summary>
    [PublicAPI]
    public class ThreadPoolSettings
    {
        public ThreadPoolSettings(int threadPoolMultiplier = 32)
        {
            ThreadPoolMultiplier = threadPoolMultiplier;
        }
        
        /// <summary>
        /// Per-core thread pool configuration multiplier.
        /// </summary>
        public int ThreadPoolMultiplier { get; set; }
    }
}