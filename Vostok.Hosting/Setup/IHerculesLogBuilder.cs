using System;
using JetBrains.Annotations;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IHerculesLogBuilder
    {
        IHerculesLogBuilder SetStream([NotNull] string stream);

        IHerculesLogBuilder SetApiKeyProvider([NotNull] Func<string> apiKeyProvider);

        IHerculesLogBuilder AddAdditionalLogTransformation([NotNull] Func<ILog, ILog> additionalTransformation);
    }
}