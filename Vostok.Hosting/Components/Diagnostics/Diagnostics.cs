using System;
using System.Collections.Generic;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Diagnostics;

namespace Vostok.Hosting.Components.Diagnostics
{
    internal class Diagnostics : IVostokApplicationDiagnostics
    {
        public IDiagnosticInfo Info =>
            throw new NotImplementedException();

        public IHealthTracker HealthTracker =>
            throw new NotImplementedException();
    }
}
