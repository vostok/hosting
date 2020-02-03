using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Hercules.Client;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokHerculesSinkBuilder
    {
        IVostokHerculesSinkBuilder Disable();

        IVostokHerculesSinkBuilder SetApiKeyProvider([NotNull] Func<string> apiKeyProvider);

        IVostokHerculesSinkBuilder SetClusterProvider([NotNull] IClusterProvider clusterProvider);
        IVostokHerculesSinkBuilder SetClusterConfigTopology([NotNull] string path);
        IVostokHerculesSinkBuilder SetServiceDiscoveryTopology([NotNull] string environment, [NotNull] string application);
        IVostokHerculesSinkBuilder SetExternalUrlTopology([NotNull] string url);

        IVostokHerculesSinkBuilder EnableVerboseLogging();
        IVostokHerculesSinkBuilder DisableVerboseLogging();

        IVostokHerculesSinkBuilder CustomizeSettings([NotNull] Action<HerculesSinkSettings> settingsCustomization);
    }
}