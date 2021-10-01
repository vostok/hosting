using System;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Diagnostics;

namespace Vostok.Hosting.Components.Diagnostics
{
    internal class DiagnosticsHub : IVostokApplicationDiagnostics, IDisposable
    {
        public DiagnosticsHub(DiagnosticInfo info, HealthTracker healthTracker)
        {
            Info = info;
            HealthTracker = healthTracker;
        }

        public DiagnosticInfo Info { get; }

        public HealthTracker HealthTracker { get; }

        public void Dispose()
        {
            HealthTracker.Dispose();
            Info.Dispose();
        }

        IDiagnosticInfo IVostokApplicationDiagnostics.Info => Info;

        IHealthTracker IVostokApplicationDiagnostics.HealthTracker => HealthTracker;
    }
}
