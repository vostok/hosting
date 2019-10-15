using System;
using JetBrains.Annotations;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokConsoleLogBuilder
    {
        IVostokConsoleLogBuilder Enable(bool synchronous = false);

        IVostokConsoleLogBuilder Disable();

        IVostokConsoleLogBuilder CustomizeLog([NotNull] Func<ILog, ILog> logCustomization);

        IVostokConsoleLogBuilder CustomizeSettings([NotNull] Action<ConsoleLogSettings> settingsCustomization);
    }
}