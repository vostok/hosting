using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Topology;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IHerculesSinkBuilder
    {
        IHerculesSinkBuilder SetApiKeyProvider([NotNull] Func<string> apiKeyProvider);
        IHerculesSinkBuilder SetClusterConfigApiKeyProvider([NotNull] string path);

        IHerculesSinkBuilder SetClusterProvider([NotNull] IClusterProvider clusterProvider);
        IHerculesSinkBuilder SetClusterConfigClusterProvider([NotNull] string path);
        IHerculesSinkBuilder SetServiceDiscoveryClusterProvider([NotNull] string environment, [NotNull] string application);
    }
}