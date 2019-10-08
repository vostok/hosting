using System;
using JetBrains.Annotations;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokHerculesLogBuilder
    {
        IVostokHerculesLogBuilder SetStream([NotNull] string stream);
        IVostokHerculesLogBuilder SetStreamFromClusterConfig([NotNull] string path);

        IVostokHerculesLogBuilder SetApiKeyProvider([NotNull] Func<string> apiKeyProvider);
        IVostokHerculesLogBuilder SetClusterConfigApiKeyProvider([NotNull] string path);

        IVostokHerculesLogBuilder AddAdditionalLogTransformation([NotNull] Func<ILog, ILog> additionalTransformation);
    }
}