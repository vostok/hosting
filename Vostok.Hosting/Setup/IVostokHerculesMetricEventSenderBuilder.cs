using System;
using JetBrains.Annotations;
using Vostok.Metrics.Hercules;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokHerculesMetricEventSenderBuilder
    {
        bool IsEnabled { get; }

        IVostokHerculesMetricEventSenderBuilder Disable();

        IVostokHerculesMetricEventSenderBuilder SetApiKeyProvider([NotNull] Func<string> apiKeyProvider, [CanBeNull] string stream = null);

        IVostokHerculesMetricEventSenderBuilder CustomizeSettings([NotNull] Action<HerculesMetricSenderSettings> settingsCustomization);
    }
}