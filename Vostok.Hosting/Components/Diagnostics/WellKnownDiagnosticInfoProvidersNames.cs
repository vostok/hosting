using JetBrains.Annotations;

namespace Vostok.Hosting.Components.Diagnostics
{
    /// <summary>
    /// Built-in hosting diagnostic info providers names.
    /// </summary>
    [PublicAPI]
    public static class WellKnownDiagnosticInfoProvidersNames
    {
        public const string EnvironmentInfo = "environment-info";

        public const string SystemMetrics = "system-metrics";

        public const string LoadedAssemblies = "loaded-assemblies";

        public const string HealthChecks = "health-checks";

        public const string Configuration = "configuration";

        public const string HerculesSink = "hercules-sink";

        public const string ApplicationMetrics = "application-metrics";

        public const string ApplicationInfo = "application-info";
    }
}