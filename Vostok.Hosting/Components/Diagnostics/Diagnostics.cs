using System;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Diagnostics;

namespace Vostok.Hosting.Components.Diagnostics
{
    internal class Diagnostics : IVostokApplicationDiagnostics
    {
        private readonly DiagnosticInfo info = new DiagnosticInfo();

        public IDiagnosticInfo Info => info;

        public IHealthTracker HealthTracker =>
            throw new NotImplementedException();
    }
}
