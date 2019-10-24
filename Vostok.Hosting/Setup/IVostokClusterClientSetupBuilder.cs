using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokClusterClientSetupBuilder
    {
        IVostokClusterClientSetupBuilder SetupTracing([NotNull] Action<IVostokClusterClientSetupTracingBuilder> tracingSetup);

        IVostokClusterClientSetupBuilder CustomizeSettings([NotNull] Action<IClusterClientConfiguration> settingsCustomization);
    }
}