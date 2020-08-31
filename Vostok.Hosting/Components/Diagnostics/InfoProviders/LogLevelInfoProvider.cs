using Vostok.Hosting.Abstractions.Diagnostics;
using Vostok.Hosting.Components.Log;

namespace Vostok.Hosting.Components.Diagnostics.InfoProviders
{
    internal class LogLevelInfoProvider : IDiagnosticInfoProvider
    {
        private readonly EventLevelCounter counter;

        public LogLevelInfoProvider(EventLevelCounter counter)
        {
            this.counter = counter;
        }

        public object Query() => counter.Collect();
    }
}