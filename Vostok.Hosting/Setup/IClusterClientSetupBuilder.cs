using JetBrains.Annotations;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IClusterClientSetupBuilder
    {
        IClusterClientSetupBuilder SetupTracing([NotNull] EnvironmentSetup<IClusterClientSetupTracingBuilder> tracingSetup);
    }
}