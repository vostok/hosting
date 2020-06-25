using System;
using JetBrains.Annotations;
using Vostok.Hosting.Components.Diagnostics;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokDiagnosticsBuilder
    {
        [NotNull]
        IVostokDiagnosticsBuilder CustomizeInfo([NotNull] Action<DiagnosticInfoSettings> customization);

        [NotNull]
        IVostokDiagnosticsBuilder CustomizeHealthTracker([NotNull] Action<HealthTrackerSettings> customization);
    }
}
