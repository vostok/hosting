using System;
using JetBrains.Annotations;
using Vostok.Metrics.Hercules;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokHerculesMetricEventSenderBuilder
    {
        IVostokHerculesMetricEventSenderBuilder SetApiKeyProvider([NotNull] Func<string> apiKeyProvider, [CanBeNull] string stream = null);

        IVostokHerculesMetricEventSenderBuilder CustomizeSettings([NotNull] Action<HerculesMetricSenderSettings> settingsCustomization);
    }
}