using System;
using Vostok.Hosting.Helpers;
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
        private bool enabled;
        private bool synchronous;

        public ConsoleLogBuilder()
        {
            settingsCustomization = new Customization<ConsoleLogSettings>();
            logCustomization = new Customization<ILog>();
        }
        
        public IVostokConsoleLogBuilder Enable(bool synchronous = false)
        {
            enabled = true;
            this.synchronous = synchronous;
            return this;
        }

        public IVostokConsoleLogBuilder Disable()
        {
            enabled = false;
            return this;
        }

        public IVostokConsoleLogBuilder CustomizeLog(Func<ILog, ILog> logCustomization)
        {
            this.logCustomization.AddCustomization(logCustomization);
            return this;
        }

        public IVostokConsoleLogBuilder CustomizeSettings(Action<ConsoleLogSettings> settingsCustomization)
        {
            this.settingsCustomization.AddCustomization(settingsCustomization);
            return this;
        }

        public ILog Build(BuildContext context)
        {
            if (!enabled)
            {
                context.Log.LogDisabled("ConsoleLog");
                return null;
            }

            var settings = new ConsoleLogSettings();

            settingsCustomization.Customize(settings);

            var log = synchronous ? (ILog)new SynchronousConsoleLog(settings) : new ConsoleLog(settings);

            logCustomization.Customize(log);

            return log;
        }
    }
}