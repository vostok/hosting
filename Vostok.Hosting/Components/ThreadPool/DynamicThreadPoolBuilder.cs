using System;
using Vostok.Hosting.Abstractions;

namespace Vostok.Hosting.Components.ThreadPool
{
    internal class DynamicThreadPoolBuilder
    {
        public void Build(BuildContext context, IVostokHostingEnvironment environment, DynamicThreadPoolSettings settings)
        {
            if (settings == null)
                context.LogDisabled("Dynamic thread pool");
            else
                BuildAndRegisterHostExtensions(context, environment, settings);
        }

        private void BuildAndRegisterHostExtensions(BuildContext context, IVostokHostingEnvironment environment, DynamicThreadPoolSettings settings)
        {
            var tracker = new DynamicThreadPoolTracker(
                settings.ChecksPeriod,
                context.ConfigurationProvider,
                context.ApplicationLimits,
                settings.ThreadPoolSettingsProvider ?? throw new ArgumentNullException(nameof(settings.ThreadPoolSettingsProvider)),
                context.Log);

            context.HostExtensions.AsMutable().Add(tracker);
            context.DisposableHostExtensions.Add(tracker);

            tracker.LaunchPeriodicalChecks(environment.ShutdownToken);
        }
    }
}