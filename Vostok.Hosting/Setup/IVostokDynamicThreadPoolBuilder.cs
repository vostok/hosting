using System;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions;
using Vostok.Hosting.Components.ThreadPool;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokDynamicThreadPoolBuilder
    {
        IVostokDynamicThreadPoolBuilder Enable();

        IVostokDynamicThreadPoolBuilder Disable();

        IVostokDynamicThreadPoolBuilder SetCheckPeriod(TimeSpan checkPeriod);

        IVostokDynamicThreadPoolBuilder SetThreadPoolProvider(Func<IConfigurationProvider, ThreadPoolSettings> provider);
        
        IVostokDynamicThreadPoolBuilder CustomizeDynamicThreadPoolSettings([NotNull] Action<DynamicThreadPoolTrackerSettings> customization);
    }
}