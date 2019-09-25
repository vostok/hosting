using System;
using JetBrains.Annotations;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IHerculesSpanSenderBuilder
    {
        IHerculesSpanSenderBuilder SetStream([NotNull] string stream);

        IHerculesSpanSenderBuilder SetApiKeyProvider([NotNull] Func<string> apiKeyProvider);
        IHerculesSpanSenderBuilder SetClusterConfigApiKeyProvider([NotNull] string path);
    }
}