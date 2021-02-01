using System;
using JetBrains.Annotations;
using Vostok.Commons.Time;
using Vostok.Configuration.Abstractions;

namespace Vostok.Hosting.Components.ThreadPool
{
    [PublicAPI]
    public class DynamicThreadPoolTrackerSettings
    {
        public TimeSpan ChecksPeriod = 10.Seconds();
        
        public Func<IConfigurationProvider, ThreadPoolSettings> ThreadPoolSettingsProvider { get; set; }
    }
}