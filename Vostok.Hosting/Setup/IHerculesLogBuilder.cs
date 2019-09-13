using System;
using JetBrains.Annotations;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IHerculesLogBuilder
    {
        IHerculesLogBuilder SetStream([NotNull] string stream);

        IHerculesLogBuilder WithAdditionalLogTransformation([NotNull] Func<ILog, ILog> additionalTransformation);
    }
}