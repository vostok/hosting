using JetBrains.Annotations;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IEnvironmentBuilder
    {
        IEnvironmentBuilder SetupLog([NotNull] EnvironmentSetup<ICompositeLogBuilder> compositeLogSetup);

        IEnvironmentBuilder SetupHerculesSink([NotNull] EnvironmentSetup<IHerculesSinkBuilder> herculesSinkSetup);

        IEnvironmentBuilder SetupClusterConfigClient([NotNull] EnvironmentSetup<IClusterConfigClientBuilder> clusterConfigClientSetup);

        IEnvironmentBuilder SetupApplicationIdentity([NotNull] EnvironmentSetup<IApplicationIdentityBuilder> applicationIdentitySetup);

        IEnvironmentBuilder SetupTracer([NotNull] EnvironmentSetup<ITracerBuilder> tracerSetup);
    }
}