using System;
using JetBrains.Annotations;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Configuration;
using Vostok.Logging.Console;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokConsoleLogBuilder
    {
        IVostokConsoleLogBuilder UseSynchronous();

        IVostokConsoleLogBuilder UseAsynchronous();

        IVostokConsoleLogBuilder Disable();

        IVostokConsoleLogBuilder SetupMinimumLevelProvider(Func<LogLevel> minLevelProvider);

        IVostokConsoleLogBuilder CustomizeLog([NotNull] Func<ILog, ILog> logCustomization);
        
        IVostokConsoleLogBuilder AddRule([NotNull] LogConfigurationRule rule);

        IVostokConsoleLogBuilder CustomizeSettings([NotNull] Action<ConsoleLogSettings> settingsCustomization);
    }
}