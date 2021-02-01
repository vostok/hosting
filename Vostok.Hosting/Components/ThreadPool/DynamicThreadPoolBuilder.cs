using System;
using Vostok.Commons.Helpers;
using Vostok.Configuration.Abstractions;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.Components.ThreadPool
{
    internal class DynamicThreadPoolBuilder : IVostokDynamicThreadPoolBuilder, IBuilder<DynamicThreadPoolTracker>
    {
        private readonly Customization<DynamicThreadPoolTrackerSettings> settingsCustomization;
        private volatile bool enabled;

        public DynamicThreadPoolTracker Build(BuildContext context)
        {
            if (!enabled)
            {
                context.LogDisabled("Dynamic thread pool");
                return null;
            }

            var customizedSettings = settingsCustomization.Customize(new DynamicThreadPoolTrackerSettings());

            return new DynamicThreadPoolTracker(
                customizedSettings.ChecksPeriod,
                context.ConfigurationProvider,
                context.ApplicationLimits,
                customizedSettings.ThreadPoolSettingsProvider,
                context.Log);
        }

        public IVostokDynamicThreadPoolBuilder Enable()
        {
            enabled = true;

            return this;
        }

        public IVostokDynamicThreadPoolBuilder Disable()
        {
            enabled = false;

            return this;
        }

        public IVostokDynamicThreadPoolBuilder SetCheckPeriod(TimeSpan checkPeriod)
        {
            settingsCustomization.AddCustomization(settings => settings.ChecksPeriod = checkPeriod);
            enabled = true;

            return this;
        }

        public IVostokDynamicThreadPoolBuilder SetThreadPoolProvider(Func<IConfigurationProvider, ThreadPoolSettings> provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            settingsCustomization.AddCustomization(settings => settings.ThreadPoolSettingsProvider = provider);
            enabled = true;

            return this;
        }

        public IVostokDynamicThreadPoolBuilder CustomizeDynamicThreadPoolSettings(Action<DynamicThreadPoolTrackerSettings> customization)
        {
            settingsCustomization.AddCustomization(customization ?? throw new ArgumentNullException(nameof(customization)));
            enabled = true;

            return this;
        }
    }
}