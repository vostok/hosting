using System;
using Vostok.Commons.Helpers;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Log
{
    internal class ConsoleLogBuilder : IVostokConsoleLogBuilder, IBuilder<ILog>
    {
        private readonly Customization<ConsoleLogSettings> settingsCustomization;
        private readonly Customization<ILog> logCustomization;
        private volatile Func<LogLevel> minLevelProvider;
        private volatile bool enabled;
        private volatile bool synchronous;

        public ConsoleLogBuilder()
        {
            settingsCustomization = new Customization<ConsoleLogSettings>();
            logCustomization = new Customization<ILog>();
        }

        public bool IsEnabled => enabled;

        public IVostokConsoleLogBuilder UseSynchronous()
        {
            synchronous = true;
            return this;
        }

        public IVostokConsoleLogBuilder UseAsynchronous()
        {
            synchronous = false;
            return this;
        }

        public IVostokConsoleLogBuilder Enable()
        {
            enabled = true;
            return this;
        }

        public IVostokConsoleLogBuilder Disable()
        {
            enabled = false;
            return this;
        }

        public IVostokConsoleLogBuilder SetupMinimumLevelProvider(Func<LogLevel> minLevelProvider)
        {
            this.minLevelProvider = minLevelProvider ?? throw new ArgumentNullException(nameof(minLevelProvider));
            return this;
        }

        public IVostokConsoleLogBuilder CustomizeLog(Func<ILog, ILog> logCustomization)
        {
            this.logCustomization.AddCustomization(logCustomization ?? throw new ArgumentNullException(nameof(logCustomization)));
            return this;
        }

        public IVostokConsoleLogBuilder CustomizeSettings(Action<ConsoleLogSettings> settingsCustomization)
        {
            this.settingsCustomization.AddCustomization(settingsCustomization ?? throw new ArgumentNullException(nameof(settingsCustomization)));
            return this;
        }

        public ILog Build(BuildContext context)
        {
            if (!enabled)
            {
                context.LogDisabled("ConsoleLog");
                return null;
            }

            var settings = new ConsoleLogSettings
            {
                ColorsEnabled = true
            };

            settingsCustomization.Customize(settings);

            var log = synchronous ? (ILog)new SynchronousConsoleLog(settings) : new ConsoleLog(settings);

            if (minLevelProvider != null)
                log = log.WithMinimumLevel(minLevelProvider);

            return logCustomization.Customize(log);
        }
    }
}