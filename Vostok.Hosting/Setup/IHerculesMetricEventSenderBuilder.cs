using System;
using JetBrains.Annotations;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IHerculesMetricEventSenderBuilder
    {
        IHerculesMetricEventSenderBuilder SetFallbackStream([NotNull] string stream);
        IHerculesMetricEventSenderBuilder SetFallbackStreamFromClusterConfig([NotNull] string path);

        IHerculesMetricEventSenderBuilder SetFinalStream([NotNull] string stream);
        IHerculesMetricEventSenderBuilder SetFinalStreamFromClusterConfig([NotNull] string path);

        IHerculesMetricEventSenderBuilder SetCountersStream([NotNull] string stream);
        IHerculesMetricEventSenderBuilder SetCountersStreamFromClusterConfig([NotNull] string path);

        IHerculesMetricEventSenderBuilder SetTimersStream([NotNull] string stream);
        IHerculesMetricEventSenderBuilder SetTimersStreamFromClusterConfig([NotNull] string path);

        IHerculesMetricEventSenderBuilder SetHistogramsStream([NotNull] string stream);
        IHerculesMetricEventSenderBuilder SetHistogramsStreamFromClusterConfig([NotNull] string path);

        IHerculesMetricEventSenderBuilder SetApiKeyProvider([NotNull] Func<string> apiKeyProvider, [CanBeNull] string stream = null);
        IHerculesMetricEventSenderBuilder SetClusterConfigApiKeyProvider([NotNull] string path, [CanBeNull] string stream = null);
    }
}