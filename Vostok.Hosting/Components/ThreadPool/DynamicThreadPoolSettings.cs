using System;
using JetBrains.Annotations;
using Vostok.Commons.Time;
using Vostok.Configuration.Abstractions;
using Vostok.Hosting.Abstractions;

namespace Vostok.Hosting.Components.ThreadPool
{
    /// <summary>
    /// Dynamic thread pool feature allows reconfiguration of per-core thread pool multiplier.
    /// Tracker retrieves <see cref="IVostokApplicationLimits.CpuUnits"/>
    /// and <see cref="ThreadPoolSettingsProvider"/> values every <see cref="ChecksPeriod"/> to reconfigure thread pool.
    /// </summary>
    [PublicAPI]
    public class DynamicThreadPoolSettings
    {
        /// <summary>
        /// Sets check period time for dynamic reconfiguration of thread pool.
        /// </summary>
        public TimeSpan ChecksPeriod = 10.Seconds();

        /// <summary>
        /// Provider of thread pool settings is called every check.
        /// </summary>
        public Func<IConfigurationProvider, ThreadPoolSettings> ThreadPoolSettingsProvider { get; set; }
    }
}