using System;
using JetBrains.Annotations;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokEnvironmentBuilder
    {
        IVostokEnvironmentBuilder SetupLog([NotNull] Action<IVostokCompositeLogBuilder> compositeLogSetup);

        IVostokEnvironmentBuilder SetupHerculesSink([NotNull] Action<IVostokHerculesSinkBuilder> herculesSinkSetup);

        IVostokEnvironmentBuilder SetupClusterConfigClient([NotNull] Action<IVostokClusterConfigClientBuilder> clusterConfigClientSetup);

        IVostokEnvironmentBuilder SetupApplicationIdentity([NotNull] Action<IVostokApplicationIdentityBuilder> applicationIdentitySetup);

        IVostokEnvironmentBuilder SetupTracer([NotNull] Action<IVostokTracerBuilder> tracerSetup);

        IVostokEnvironmentBuilder SetupMetrics([NotNull] Action<IVostokMetricsBuilder> metricsSetup);

        IVostokEnvironmentBuilder SetupClusterClientSetup([NotNull] Action<IVostokClusterClientSetupBuilder> clusterClientSetup);

        IVostokEnvironmentBuilder SetupZooKeeperClient([NotNull] Action<IVostokZooKeeperClientBuilder> zooKeeperClientSetup);

        IVostokEnvironmentBuilder SetupServiceBeacon([NotNull] Action<IVostokServiceBeaconBuilder> serviceBeaconSetup);

        IVostokEnvironmentBuilder SetupServiceLocator([NotNull] Action<IVostokServiceLocatorBuilder> serviceLocatorSetup);
    }
}