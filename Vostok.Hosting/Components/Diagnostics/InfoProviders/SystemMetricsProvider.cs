using Vostok.Commons.Threading;
using Vostok.Hosting.Abstractions.Diagnostics;

namespace Vostok.Hosting.Components.Diagnostics.InfoProviders
{
    internal class SystemMetricsProvider : IDiagnosticInfoProvider
    {
        public object Query() => new
        {
            ThreadPool = ThreadPoolUtility.GetPoolState(),
        };
    }
}
