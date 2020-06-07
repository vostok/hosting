using JetBrains.Annotations;

namespace Vostok.Hosting.Components.Diagnostics
{
    [PublicAPI]
    public class DiagnosticInfoSettings
    {
        public bool AddEnvironmentInfo { get; set; } = true;

        public bool AddLoadedAssembliesInfo { get; set; } = true;
        
        public bool AddSystemMetricsInfo { get; set; } = true;

        public bool AddHealthChecksInfo { get; set; } = true;

        public bool AddConfigurationInfo { get; set; }

        public bool AddHerculesSinkInfo { get; set; } = true;
    }
}
