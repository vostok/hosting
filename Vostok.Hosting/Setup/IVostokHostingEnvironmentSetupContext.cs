using JetBrains.Annotations;
using Vostok.ClusterConfig.Client.Abstractions;
using Vostok.Configuration.Abstractions;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokHostingEnvironmentSetupContext
    {
        [NotNull]
        ILog Log { get; }

        [NotNull]
        IConfigurationSource ConfigurationSource { get; }

        [NotNull]
        IConfigurationProvider ConfigurationProvider { get; }

        [NotNull]
        IClusterConfigClient ClusterConfigClient { get; }
    }
}