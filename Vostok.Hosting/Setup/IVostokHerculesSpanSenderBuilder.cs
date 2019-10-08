using System;
using JetBrains.Annotations;
using Vostok.Tracing.Hercules;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokHerculesSpanSenderBuilder
    {
        IVostokHerculesSpanSenderBuilder SetStream([NotNull] string stream);
        IVostokHerculesSpanSenderBuilder SetStreamFromClusterConfig([NotNull] string path);

        IVostokHerculesSpanSenderBuilder SetApiKeyProvider([NotNull] Func<string> apiKeyProvider);
        IVostokHerculesSpanSenderBuilder SetClusterConfigApiKeyProvider([NotNull] string path);

        IVostokHerculesSpanSenderBuilder CustomizeSettings([NotNull] Action<HerculesSpanSenderSettings> settingsCustomization);
    }
}