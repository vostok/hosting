using JetBrains.Annotations;
using Vostok.Hosting.Abstractions;

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
        public int ThreadPoolMultiplier { get; set; }

        /// <summary>
        /// It is used for reconfiguration when on <see cref="IVostokApplicationLimits"/> change.
        /// Actual value of this setting is <see cref="IVostokApplicationLimits.CpuUnits"/>.
        /// </summary>
        internal float? CpuUnits { get; set; }
    }
}