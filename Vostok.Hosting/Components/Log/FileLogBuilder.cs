using System;
using System.IO;
using Vostok.Commons.Helpers;
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
        private volatile Func<FileLogSettings> settingsProvider;
        private volatile Func<LogLevel> minLevelProvider;
        private volatile bool enabled;

        public FileLogBuilder()
        {
            settingsCustomization = new Customization<FileLogSettings>();
            logCustomization = new Customization<ILog>();
        }

        public bool IsEnabled => enabled;

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

        public IVostokFileLogBuilder SetupMinimumLevelProvider(Func<LogLevel> minLevelProvider)
        {
            this.minLevelProvider = minLevelProvider ?? throw new ArgumentNullException(nameof(minLevelProvider));
            return this;
        }

        public IVostokFileLogBuilder CustomizeLog(Func<ILog, ILog> logCustomization)
        {
            this.logCustomization.AddCustomization(logCustomization ?? throw new ArgumentNullException(nameof(logCustomization)));
            return this;
        }

        public IVostokFileLogBuilder CustomizeSettings(Action<FileLogSettings> settingsCustomization)
        {
            this.settingsCustomization.AddCustomization(settingsCustomization ?? throw new ArgumentNullException(nameof(settingsCustomization)));
            return this;
        }

        public IVostokFileLogBuilder SetSettingsProvider(Func<FileLogSettings> settingsProvider)
        {
            this.settingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));
            return this;
        }

        public ILog Build(BuildContext context)
        {
            if (!enabled)
            {
                context.LogDisabled("FileLog");
                return null;
            }

            if (settingsProvider == null)
            {
                var settings = new FileLogSettings();
                settingsCustomization.Customize(settings);
                settingsProvider = () => settings;
            }

            context.LogsDirectory = GetLogsDirectory();

            ILog log = new FileLog(settingsProvider);

            if (minLevelProvider != null)
                log = log.WithMinimumLevel(minLevelProvider);

            return logCustomization.Customize(log);
        }

        private string GetLogsDirectory()
        {
            try
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, settingsProvider().FilePath);
                return Path.GetDirectoryName(path);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}