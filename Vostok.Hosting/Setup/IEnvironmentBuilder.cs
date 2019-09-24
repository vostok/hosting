using System.Threading;
using JetBrains.Annotations;
using Vostok.Hosting.Components.Log;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IEnvironmentBuilder
    {
        IEnvironmentBuilder SetupLog([NotNull] EnvironmentSetup<ICompositeLogBuilder> compositeLogSetup);

        IEnvironmentBuilder SetupHerculesSink([NotNull] EnvironmentSetup<IHerculesSinkBuilder> herculesSinkSetup);

        IEnvironmentBuilder SetupClusterConfigClient([NotNull] EnvironmentSetup<IClusterConfigClientBuilder> clusterConfigClientSetup);

        IEnvironmentBuilder SetupApplicationIdentity([NotNull] EnvironmentSetup<IApplicationIdentityBuilder> applicationIdentitySetup);
    }
}