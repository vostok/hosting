using System;
using JetBrains.Annotations;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokHerculesMetricEventSenderBuilder
    {
        IVostokHerculesMetricEventSenderBuilder SetFallbackStream([NotNull] string stream);
        IVostokHerculesMetricEventSenderBuilder SetFallbackStreamFromClusterConfig([NotNull] string path);

        IVostokHerculesMetricEventSenderBuilder SetFinalStream([NotNull] string stream);
        IVostokHerculesMetricEventSenderBuilder SetFinalStreamFromClusterConfig([NotNull] string path);

        IVostokHerculesMetricEventSenderBuilder SetCountersStream([NotNull] string stream);
        IVostokHerculesMetricEventSenderBuilder SetCountersStreamFromClusterConfig([NotNull] string path);

        IVostokHerculesMetricEventSenderBuilder SetTimersStream([NotNull] string stream);
        IVostokHerculesMetricEventSenderBuilder SetTimersStreamFromClusterConfig([NotNull] string path);

        IVostokHerculesMetricEventSenderBuilder SetHistogramsStream([NotNull] string stream);
        IVostokHerculesMetricEventSenderBuilder SetHistogramsStreamFromClusterConfig([NotNull] string path);

        IVostokHerculesMetricEventSenderBuilder SetApiKeyProvider([NotNull] Func<string> apiKeyProvider, [CanBeNull] string stream = null);
        IVostokHerculesMetricEventSenderBuilder SetClusterConfigApiKeyProvider([NotNull] string path, [CanBeNull] string stream = null);
    }
}