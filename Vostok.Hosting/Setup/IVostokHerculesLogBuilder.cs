using System;
using JetBrains.Annotations;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Hercules.Configuration;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokHerculesLogBuilder
    {
        IVostokHerculesLogBuilder SetStream([NotNull] string stream);
        IVostokHerculesLogBuilder SetStreamFromClusterConfig([NotNull] string path);

        IVostokHerculesLogBuilder SetApiKeyProvider([NotNull] Func<string> apiKeyProvider);
        IVostokHerculesLogBuilder SetClusterConfigApiKeyProvider([NotNull] string path);

        IVostokHerculesLogBuilder CustomizeLog([NotNull] Func<ILog, ILog> additionalTransformation);

        IVostokHerculesLogBuilder CustomizeSettings([NotNull] Action<HerculesLogSettings> settingsCustomization);
    }
}