using System;
using JetBrains.Annotations;
using Vostok.Tracing.Hercules;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokHerculesSpanSenderBuilder
    {
        IVostokHerculesSpanSenderBuilder Disable();

        IVostokHerculesSpanSenderBuilder SetStream([NotNull] string stream);

        IVostokHerculesSpanSenderBuilder SetApiKeyProvider([NotNull] Func<string> apiKeyProvider);

        IVostokHerculesSpanSenderBuilder CustomizeSettings([NotNull] Action<HerculesSpanSenderSettings> settingsCustomization);
    }
}