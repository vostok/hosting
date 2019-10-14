using System;
using JetBrains.Annotations;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokCompositeLogBuilder
    {
        IVostokCompositeLogBuilder AddLog([NotNull] ILog log);

        IVostokCompositeLogBuilder SetupFileLog([NotNull] Action<IVostokFileLogBuilder> fileLogSetup);

        IVostokCompositeLogBuilder SetupHerculesLog([NotNull] Action<IVostokHerculesLogBuilder> herculesLogSetup);
    }
}