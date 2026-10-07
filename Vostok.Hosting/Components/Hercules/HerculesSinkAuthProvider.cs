using JetBrains.Annotations;

namespace Vostok.Hosting.Components.Hercules
{
    [PublicAPI]
    public static class HerculesSinkAuthProvider
    {
        [CanBeNull]
        public static string ApiKey { get; set; }

        [CanBeNull]
        public static string CustomLoggingApiKey { get; set; }

        [CanBeNull]
        public static string CustomMetricsApiKey { get; set; }

        [CanBeNull]
        public static string CustomTracingApiKey { get; set; }

        [CanBeNull]
        public static string CustomServiceDiscoveryEventsApiKey { get; set; }

        public static bool NeedConfigureCustomKeys() => string.IsNullOrEmpty(ApiKey);
    }
}