using System;
using JetBrains.Annotations;

namespace Vostok.Hosting.Setup
{
    // CR(iloktionov): Consistent naming
    [PublicAPI]
    public interface IEnvironmentBuilder
    {
        IEnvironmentBuilder SetupLog([NotNull] Action<ICompositeLogBuilder> compositeLogSetup);

        IEnvironmentBuilder SetupHerculesSink([NotNull] Action<IHerculesSinkBuilder> herculesSinkSetup);

        IEnvironmentBuilder SetupClusterConfigClient([NotNull] Action<IClusterConfigClientBuilder> clusterConfigClientSetup);

        IEnvironmentBuilder SetupApplicationIdentity([NotNull] Action<IApplicationIdentityBuilder> applicationIdentitySetup);

        IEnvironmentBuilder SetupTracer([NotNull] Action<ITracerBuilder> tracerSetup);

        IEnvironmentBuilder SetupMetrics([NotNull] Action<IMetricsBuilder> metricsSetup);

        IEnvironmentBuilder SetupClusterClientSetup([NotNull] Action<IClusterClientSetupBuilder> clusterClientSetup);

        IEnvironmentBuilder SetupZooKeeperClient([NotNull] Action<IZooKeeperClientBuilder> zooKeeperClientSetup);

        IEnvironmentBuilder SetupServiceBeacon([NotNull] Action<IServiceBeaconBuilder> serviceBeaconSetup);

        IEnvironmentBuilder SetupServiceLocator([NotNull] Action<IServiceLocatorBuilder> serviceLocatorSetup);
    }
}