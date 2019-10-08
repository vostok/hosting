using System;
using JetBrains.Annotations;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokCompositeLogBuilder
    {
        IVostokCompositeLogBuilder AddLog([NotNull] ILog log);

        //ICompositeLogBuilder AddFileLog([CanBeNull] Action<> fileLogSettings = null);

        IVostokCompositeLogBuilder SetupHerculesLog([NotNull] Action<IVostokHerculesLogBuilder> herculesLogSetup);
    }
}