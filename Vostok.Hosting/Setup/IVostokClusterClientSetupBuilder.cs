using System;
using JetBrains.Annotations;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokClusterClientSetupBuilder
    {
        IVostokClusterClientSetupBuilder SetupTracing([NotNull] Action<IVostokClusterClientSetupTracingBuilder> tracingSetup);
    }
}