using System;
using JetBrains.Annotations;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokEnvironmentBuilder
    {
        IVostokEnvironmentBuilder SetupLog([NotNull] Action<IVostokCompositeLogBuilder> compositeLogSetup);

        IVostokEnvironmentBuilder SetupClusterConfigClient([NotNull] Action<IVostokClusterConfigClientBuilder> clusterConfigClientSetup);

        IVostokEnvironmentBuilder SetupConfiguration([NotNull] Action<IVostokConfigurationBuilder> configurationSetup);

        IVostokEnvironmentBuilder SetupHerculesSink([NotNull] Action<IVostokHerculesSinkBuilder> herculesSinkSetup);
        IVostokEnvironmentBuilder SetupHerculesSink([NotNull] Action<IVostokHerculesSinkBuilder, IVostokConfigurationContext> herculesSinkSetup);
        
        IVostokEnvironmentBuilder SetupApplicationIdentity([NotNull] Action<IVostokApplicationIdentityBuilder> applicationIdentitySetup);
        IVostokEnvironmentBuilder SetupApplicationIdentity([NotNull] Action<IVostokApplicationIdentityBuilder, IVostokConfigurationContext> applicationIdentitySetup);

        IVostokEnvironmentBuilder SetupTracer([NotNull] Action<IVostokTracerBuilder> tracerSetup);
        IVostokEnvironmentBuilder SetupTracer([NotNull] Action<IVostokTracerBuilder, IVostokConfigurationContext> tracerSetup);

        IVostokEnvironmentBuilder SetupMetrics([NotNull] Action<IVostokMetricsBuilder> metricsSetup);
        IVostokEnvironmentBuilder SetupMetrics([NotNull] Action<IVostokMetricsBuilder, IVostokConfigurationContext> metricsSetup);

        IVostokEnvironmentBuilder SetupClusterClientSetup([NotNull] Action<IVostokClusterClientSetupBuilder> clusterClientSetup);
        IVostokEnvironmentBuilder SetupClusterClientSetup([NotNull] Action<IVostokClusterClientSetupBuilder, IVostokConfigurationContext> clusterClientSetup);

        IVostokEnvironmentBuilder SetupZooKeeperClient([NotNull] Action<IVostokZooKeeperClientBuilder> zooKeeperClientSetup);
        IVostokEnvironmentBuilder SetupZooKeeperClient([NotNull] Action<IVostokZooKeeperClientBuilder, IVostokConfigurationContext> zooKeeperClientSetup);

        IVostokEnvironmentBuilder SetupServiceBeacon([NotNull] Action<IVostokServiceBeaconBuilder> serviceBeaconSetup);
        IVostokEnvironmentBuilder SetupServiceBeacon([NotNull] Action<IVostokServiceBeaconBuilder, IVostokConfigurationContext> serviceBeaconSetup);

        IVostokEnvironmentBuilder SetupServiceLocator([NotNull] Action<IVostokServiceLocatorBuilder> serviceLocatorSetup);
        IVostokEnvironmentBuilder SetupServiceLocator([NotNull] Action<IVostokServiceLocatorBuilder, IVostokConfigurationContext> serviceLocatorSetup);
    }
}