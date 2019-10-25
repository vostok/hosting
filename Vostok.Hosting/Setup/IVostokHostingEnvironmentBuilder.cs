using System;
using JetBrains.Annotations;
using Vostok.Hosting.Abstractions;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokHostingEnvironmentBuilder
    {
        IVostokHostingEnvironmentBuilder SetupClusterConfigClient([NotNull] Action<IVostokClusterConfigClientBuilder> setup);

        IVostokHostingEnvironmentBuilder SetupConfiguration([NotNull] Action<IVostokConfigurationBuilder> setup);

        IVostokHostingEnvironmentBuilder SetupLog([NotNull] Action<IVostokCompositeLogBuilder> setup);
        IVostokHostingEnvironmentBuilder SetupLog([NotNull] Action<IVostokCompositeLogBuilder, IVostokHostingEnvironmentSetupContext> setup);

        IVostokHostingEnvironmentBuilder SetupHerculesSink([NotNull] Action<IVostokHerculesSinkBuilder> setup);
        IVostokHostingEnvironmentBuilder SetupHerculesSink([NotNull] Action<IVostokHerculesSinkBuilder, IVostokHostingEnvironmentSetupContext> setup);

        IVostokHostingEnvironmentBuilder SetupApplicationIdentity([NotNull] Action<IVostokApplicationIdentityBuilder> setup);
        IVostokHostingEnvironmentBuilder SetupApplicationIdentity([NotNull] Action<IVostokApplicationIdentityBuilder, IVostokHostingEnvironmentSetupContext> setup);

        IVostokHostingEnvironmentBuilder SetupApplicationLimits([NotNull] Action<IVostokApplicationLimitsBuilder> setup);
        IVostokHostingEnvironmentBuilder SetupApplicationLimits([NotNull] Action<IVostokApplicationLimitsBuilder, IVostokHostingEnvironmentSetupContext> setup);

        IVostokHostingEnvironmentBuilder SetupApplicationReplicationInfo([NotNull] Action<IVostokApplicationReplicationInfoBuilder> setup);
        IVostokHostingEnvironmentBuilder SetupApplicationReplicationInfo([NotNull] Action<IVostokApplicationReplicationInfoBuilder, IVostokHostingEnvironmentSetupContext> setup);

        IVostokHostingEnvironmentBuilder SetupTracer([NotNull] Action<IVostokTracerBuilder> setup);
        IVostokHostingEnvironmentBuilder SetupTracer([NotNull] Action<IVostokTracerBuilder, IVostokHostingEnvironmentSetupContext> setup);

        IVostokHostingEnvironmentBuilder SetupMetrics([NotNull] Action<IVostokMetricsBuilder> setup);
        IVostokHostingEnvironmentBuilder SetupMetrics([NotNull] Action<IVostokMetricsBuilder, IVostokHostingEnvironmentSetupContext> setup);

        IVostokHostingEnvironmentBuilder SetupClusterClientSetup([NotNull] Action<IVostokClusterClientSetupBuilder> setup);
        IVostokHostingEnvironmentBuilder SetupClusterClientSetup([NotNull] Action<IVostokClusterClientSetupBuilder, IVostokHostingEnvironmentSetupContext> setup);

        IVostokHostingEnvironmentBuilder SetupZooKeeperClient([NotNull] Action<IVostokZooKeeperClientBuilder> setup);
        IVostokHostingEnvironmentBuilder SetupZooKeeperClient([NotNull] Action<IVostokZooKeeperClientBuilder, IVostokHostingEnvironmentSetupContext> setup);

        IVostokHostingEnvironmentBuilder SetupServiceBeacon([NotNull] Action<IVostokServiceBeaconBuilder> setup);
        IVostokHostingEnvironmentBuilder SetupServiceBeacon([NotNull] Action<IVostokServiceBeaconBuilder, IVostokHostingEnvironmentSetupContext> setup);

        IVostokHostingEnvironmentBuilder SetupServiceLocator([NotNull] Action<IVostokServiceLocatorBuilder> setup);
        IVostokHostingEnvironmentBuilder SetupServiceLocator([NotNull] Action<IVostokServiceLocatorBuilder, IVostokHostingEnvironmentSetupContext> setup);

        IVostokHostingEnvironmentBuilder SetupHostExtensions([NotNull] Action<IVostokHostExtensionsBuilder> setup);
        IVostokHostingEnvironmentBuilder SetupHostExtensions([NotNull] Action<IVostokHostExtensionsBuilder, IVostokHostingEnvironment> setup);
    }
}