﻿using System;
using JetBrains.Annotations;
using Vostok.Hosting.Abstractions;

namespace Vostok.Hosting.Setup
{
    /// <summary>
    /// <para>A main builder of <see cref="IVostokHostingEnvironment"/>.</para>
    /// <para>Use it to configure all components.</para>
    /// <para>Uses following design principles:</para>
    /// <list type="bullet">
    ///     <item><description>All components have to be manually configured from scratch.</description></item>
    ///     <item><description>If some component has not been configured, info message will be printed on console or built log.</description></item>
    ///     <item><description>If some error has occured during environment building, error message will be printed on console or built log.</description></item>
    ///     <item><description>Full configuration of each component implementation is available via settings customization.</description></item>
    /// </list>
    /// </summary>
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

        IVostokHostingEnvironmentBuilder SetupDatacenters([NotNull] Action<IVostokDatacentersBuilder> setup);
        IVostokHostingEnvironmentBuilder SetupDatacenters([NotNull] Action<IVostokDatacentersBuilder, IVostokHostingEnvironmentSetupContext> setup);

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