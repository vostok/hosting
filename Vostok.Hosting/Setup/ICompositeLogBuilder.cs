using JetBrains.Annotations;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface ICompositeLogBuilder
    {
        ICompositeLogBuilder AddLog([NotNull] ILog log);

        //ICompositeLogBuilder AddFileLog([CanBeNull] EnvironmentSetup<> fileLogSettings = null);

        ICompositeLogBuilder AddHerculesLog([NotNull] EnvironmentSetup<IHerculesLogBuilder> herculesLogSetup);
    }
}