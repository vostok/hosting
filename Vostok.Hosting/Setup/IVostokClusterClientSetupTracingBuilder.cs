using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Tracing;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokClusterClientSetupTracingBuilder
    {
        IVostokClusterClientSetupTracingBuilder CustomizeSettings([NotNull] Action<TracingConfiguration> settingsCustomization);
    }
}