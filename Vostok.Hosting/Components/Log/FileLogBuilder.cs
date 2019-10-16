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
        private Func<FileLogSettings> settingsProvider;
        private bool enabled;

        public FileLogBuilder()
        {
            settingsCustomization = new Customization<FileLogSettings>();
            logCustomization = new Customization<ILog>();
        }

        public IVostokFileLogBuilder Enable()
        {
            enabled = true;
            return this;
        }

        public IVostokFileLogBuilder Disable()
        {
            enabled = false;
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

        public IVostokFileLogBuilder SetSettingsProvider(Func<FileLogSettings> settingsProvider)
        {
            this.settingsProvider = settingsProvider;
            return this;
        }

        public ILog Build(BuildContext context)
        {
            if (!enabled)
            {
                context.Log.LogDisabled("FileLog");
                return null;
            }

            if (settingsProvider == null)
            {
                var settings = new FileLogSettings();
                settingsCustomization.Customize(settings);
                settingsProvider = () => settings;
            }

            var log = new FileLog(settingsProvider);

            logCustomization.Customize(log);

            return log;
        }
    }
}