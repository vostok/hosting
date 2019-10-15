using System;
using JetBrains.Annotations;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokHostingEnvironmentBuilder
    {
        IVostokHostingEnvironmentBuilder SetupClusterConfigClient([NotNull] Action<IVostokClusterConfigClientBuilder> clusterConfigClientSetup);

        IVostokHostingEnvironmentBuilder SetupConfiguration([NotNull] Action<IVostokConfigurationBuilder> configurationSetup);

        IVostokHostingEnvironmentBuilder SetupLog([NotNull] Action<IVostokCompositeLogBuilder> compositeLogSetup);
        IVostokHostingEnvironmentBuilder SetupLog([NotNull] Action<IVostokCompositeLogBuilder, IVostokHostingEnvironmentSetupContext> compositeLogSetup);

        IVostokHostingEnvironmentBuilder SetupHerculesSink([NotNull] Action<IVostokHerculesSinkBuilder> herculesSinkSetup);
        IVostokHostingEnvironmentBuilder SetupHerculesSink([NotNull] Action<IVostokHerculesSinkBuilder, IVostokHostingEnvironmentSetupContext> herculesSinkSetup);
        
        IVostokHostingEnvironmentBuilder SetupApplicationIdentity([NotNull] Action<IVostokApplicationIdentityBuilder> applicationIdentitySetup);
        IVostokHostingEnvironmentBuilder SetupApplicationIdentity([NotNull] Action<IVostokApplicationIdentityBuilder, IVostokHostingEnvironmentSetupContext> applicationIdentitySetup);

        IVostokHostingEnvironmentBuilder SetupTracer([NotNull] Action<IVostokTracerBuilder> tracerSetup);
        IVostokHostingEnvironmentBuilder SetupTracer([NotNull] Action<IVostokTracerBuilder, IVostokHostingEnvironmentSetupContext> tracerSetup);

        IVostokHostingEnvironmentBuilder SetupMetrics([NotNull] Action<IVostokMetricsBuilder> metricsSetup);
        IVostokHostingEnvironmentBuilder SetupMetrics([NotNull] Action<IVostokMetricsBuilder, IVostokHostingEnvironmentSetupContext> metricsSetup);

        IVostokHostingEnvironmentBuilder SetupClusterClientSetup([NotNull] Action<IVostokClusterClientSetupBuilder> clusterClientSetup);
        IVostokHostingEnvironmentBuilder SetupClusterClientSetup([NotNull] Action<IVostokClusterClientSetupBuilder, IVostokHostingEnvironmentSetupContext> clusterClientSetup);

        IVostokHostingEnvironmentBuilder SetupZooKeeperClient([NotNull] Action<IVostokZooKeeperClientBuilder> zooKeeperClientSetup);
        IVostokHostingEnvironmentBuilder SetupZooKeeperClient([NotNull] Action<IVostokZooKeeperClientBuilder, IVostokHostingEnvironmentSetupContext> zooKeeperClientSetup);

        IVostokHostingEnvironmentBuilder SetupServiceBeacon([NotNull] Action<IVostokServiceBeaconBuilder> serviceBeaconSetup);
        IVostokHostingEnvironmentBuilder SetupServiceBeacon([NotNull] Action<IVostokServiceBeaconBuilder, IVostokHostingEnvironmentSetupContext> serviceBeaconSetup);

        IVostokHostingEnvironmentBuilder SetupServiceLocator([NotNull] Action<IVostokServiceLocatorBuilder> serviceLocatorSetup);
        IVostokHostingEnvironmentBuilder SetupServiceLocator([NotNull] Action<IVostokServiceLocatorBuilder, IVostokHostingEnvironmentSetupContext> serviceLocatorSetup);

        IVostokHostingEnvironmentBuilder SetupHostExtensions([NotNull] Action<IVostokHostExtensionsBuilder> hostExtensionsSetup);
        IVostokHostingEnvironmentBuilder SetupHostExtensions([NotNull] Action<IVostokHostExtensionsBuilder, IVostokHostingEnvironmentSetupContext> hostExtensionsSetup);
    }
}