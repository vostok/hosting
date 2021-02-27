using System;
using JetBrains.Annotations;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Hercules.Configuration;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokHerculesLogBuilder
    {
        IVostokHerculesLogBuilder Disable();

        IVostokHerculesLogBuilder SetStream([NotNull] string stream);

        IVostokHerculesLogBuilder SetApiKeyProvider([NotNull] Func<string> apiKeyProvider);

        IVostokHerculesLogBuilder SetupMinimumLevelProvider(Func<LogLevel> minLevelProvider);

        IVostokHerculesLogBuilder CustomizeLog([NotNull] Func<ILog, ILog> logCustomization);

        IVostokHerculesLogBuilder CustomizeSettings([NotNull] Action<HerculesLogSettings> settingsCustomization);
    }
}