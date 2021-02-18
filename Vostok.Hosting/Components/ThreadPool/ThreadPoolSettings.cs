using JetBrains.Annotations;

namespace Vostok.Hosting.Components.ThreadPool
{
    /// <summary>
    /// Settings class that is used to dynamically configure thread pool.
    /// </summary>
    [PublicAPI]
    public class ThreadPoolSettings
    {
        /// <summary>
        /// Per-core thread pool configuration multiplier.
        /// </summary>
        public int ThreadPoolMultiplier { get; set; } = 32;
    }
}