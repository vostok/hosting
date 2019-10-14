using System;
using Vostok.Hosting.Helpers;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.Logging.File;
using Vostok.Logging.File.Configuration;
// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Log
{
    internal class FileLogBuilder : IVostokFileLogBuilder, IBuilder<ILog>
    {
        private readonly Customization<FileLogSettings> settingsCustomization;
        private readonly Customization<ILog> logCustomization;
        private bool disabled;

        public FileLogBuilder()
        {
            settingsCustomization = new Customization<FileLogSettings>();
            logCustomization = new Customization<ILog>();
        }

        public IVostokFileLogBuilder Enable()
        {
            disabled = false;
            return this;
        }

        public IVostokFileLogBuilder Disable()
        {
            disabled = true;
            return this;
        }

        public IVostokFileLogBuilder CustomizeLog(Func<ILog, ILog> logCustomization)
        {
            this.logCustomization.AddCustomization(logCustomization);
            return this;
        }

        public IVostokFileLogBuilder CustomizeSettings(Action<FileLogSettings> settingsCustomization)
        {
            this.settingsCustomization.AddCustomization(settingsCustomization);
            return this;
        }

        public ILog Build(BuildContext context)
        {
            if (disabled)
                return null;

            var settings = new FileLogSettings();

            settingsCustomization.Customize(settings);

            var log = new FileLog(settings);

            logCustomization.Customize(log);

            return log;
        }
    }
}