using JetBrains.Annotations;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IHostingLogBuilder
    {
        IHostingLogBuilder AddLog(ILog log);
    }
}