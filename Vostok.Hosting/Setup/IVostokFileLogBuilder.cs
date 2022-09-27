using System;
using JetBrains.Annotations;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Configuration;
using Vostok.Logging.File.Configuration;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokFileLogBuilder
    {
        bool IsEnabled { get; }

        IVostokFileLogBuilder Enable();

        IVostokFileLogBuilder Disable();

        IVostokFileLogBuilder SetupMinimumLevelProvider(Func<LogLevel> minLevelProvider);

        IVostokFileLogBuilder CustomizeLog([NotNull] Func<ILog, ILog> logCustomization);

        IVostokFileLogBuilder AddRule([NotNull] LogConfigurationRule rule);

        IVostokFileLogBuilder CustomizeSettings([NotNull] Action<FileLogSettings> settingsCustomization);

        IVostokFileLogBuilder SetSettingsProvider([NotNull] Func<FileLogSettings> settingsProvider);

        IVostokFileLogBuilder DisposeWithEnvironment(bool value);
    }
}