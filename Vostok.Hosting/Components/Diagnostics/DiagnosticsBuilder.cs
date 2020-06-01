using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.Components.Diagnostics
{
    internal class DiagnosticsBuilder : IVostokDiagnosticsBuilder, IBuilder<IVostokApplicationDiagnostics>
    {
        public IVostokApplicationDiagnostics Build(BuildContext context) => new Diagnostics();
    }
}
