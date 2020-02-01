using System;
using JetBrains.Annotations;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokCompositeLogBuilder
    {
        IVostokCompositeLogBuilder AddLog([NotNull] ILog log);

        IVostokCompositeLogBuilder AddLog([NotNull] string name, [NotNull] ILog log);

        IVostokCompositeLogBuilder CustomizeLog([NotNull] Func<ILog, ILog> logCustomization);

        IVostokCompositeLogBuilder SetupFileLog([NotNull] Action<IVostokFileLogBuilder> fileLogSetup);

        IVostokCompositeLogBuilder SetupConsoleLog([NotNull] Action<IVostokConsoleLogBuilder> consoleLogSetup);

        IVostokCompositeLogBuilder SetupHerculesLog([NotNull] Action<IVostokHerculesLogBuilder> herculesLogSetup);
    }
}