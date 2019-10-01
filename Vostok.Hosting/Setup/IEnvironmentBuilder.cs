using JetBrains.Annotations;
using Vostok.Clusterclient.Core;

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

        IEnvironmentBuilder SetupMetrics([NotNull] EnvironmentSetup<IMetricsBuilder> metricsSetup);

        IEnvironmentBuilder SetupClusterClient([NotNull] ClusterClientSetup clusterClientSetup);
    }
}