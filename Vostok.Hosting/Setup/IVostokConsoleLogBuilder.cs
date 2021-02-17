using System;
using JetBrains.Annotations;
using Vostok.Logging.Abstractions;
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

        IVostokConsoleLogBuilder CustomizeSettings([NotNull] Action<ConsoleLogSettings> settingsCustomization);
    }
}