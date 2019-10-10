using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Hercules.Client;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokHerculesSinkBuilder
    {
        IVostokHerculesSinkBuilder SetApiKeyProvider([NotNull] Func<string> apiKeyProvider);

        IVostokHerculesSinkBuilder SetClusterProvider([NotNull] IClusterProvider clusterProvider);
        IVostokHerculesSinkBuilder SetClusterConfigClusterProvider([NotNull] string path);
        IVostokHerculesSinkBuilder SetServiceDiscoveryClusterProvider([NotNull] string environment, [NotNull] string application);

        IVostokHerculesSinkBuilder SuppressVerboseLogging();

        IVostokHerculesSinkBuilder CustomizeSettings([NotNull] Action<HerculesSinkSettings> settingsCustomization);
    }
}