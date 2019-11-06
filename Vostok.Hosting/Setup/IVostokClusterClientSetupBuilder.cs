using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Ordering.Weighed;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokClusterClientSetupBuilder
    {
        IVostokClusterClientSetupBuilder SetupTracing([NotNull] Action<IVostokClusterClientSetupTracingBuilder> tracingSetup);

        IVostokClusterClientSetupBuilder CustomizeSettings([NotNull] Action<IClusterClientConfiguration> settingsCustomization);

        IVostokClusterClientSetupBuilder CustomizeWeightOrdering([NotNull] Action<IWeighedReplicaOrderingBuilder> weightOrderingCustomization);
    }
}