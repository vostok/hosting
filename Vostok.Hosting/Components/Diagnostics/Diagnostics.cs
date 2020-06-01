using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Diagnostics;

namespace Vostok.Hosting.Components.Diagnostics
{
    internal class Diagnostics : IVostokApplicationDiagnostics
    {
        private readonly DiagnosticInfo info = new DiagnosticInfo();

        private readonly HealthTracker healthTracker = new HealthTracker();

        public IDiagnosticInfo Info => info;

        public IHealthTracker HealthTracker => healthTracker;
    }
}
