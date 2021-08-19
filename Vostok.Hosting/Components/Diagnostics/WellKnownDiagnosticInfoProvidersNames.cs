using JetBrains.Annotations;

namespace Vostok.Hosting.Components.Diagnostics
{
    /// <summary>
    /// Built-in hosting diagnostic info providers names.
    /// </summary>
    [PublicAPI]
    public class WellKnownDiagnosticInfoProvidersNames
    {
        public static string EnvironmentInfo = "environment-info";

        public static string SystemMetrics = "system-metrics";

        public static string LoadedAssemblies = "loaded-assemblies";

        public static string HealthChecks = "health-checks";

        public static string Configuration = "configuration";

        public static string HerculesSink = "hercules-sink";

        public static string ApplicationMetrics = "application-metrics";

        public static string ApplicationInfo = "application-info";
    }
}