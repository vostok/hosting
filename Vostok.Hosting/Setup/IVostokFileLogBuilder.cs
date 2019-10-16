using System;
using JetBrains.Annotations;
using Vostok.Logging.Abstractions;
using Vostok.Logging.File.Configuration;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokFileLogBuilder
    {
        IVostokFileLogBuilder Disable();

        IVostokFileLogBuilder CustomizeLog([NotNull] Func<ILog, ILog> logCustomization);

        IVostokFileLogBuilder CustomizeSettings([NotNull] Action<FileLogSettings> settingsCustomization);
    }
}