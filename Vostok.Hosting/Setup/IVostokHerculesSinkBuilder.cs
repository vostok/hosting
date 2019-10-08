using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Topology;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokHerculesSinkBuilder
    {
        IVostokHerculesSinkBuilder SetApiKeyProvider([NotNull] Func<string> apiKeyProvider);

        // CR(iloktionov): Get rid of CC special case (globally). Use configuration abstractions instead.
        IVostokHerculesSinkBuilder SetClusterConfigApiKeyProvider([NotNull] string path);

        IVostokHerculesSinkBuilder SetClusterProvider([NotNull] IClusterProvider clusterProvider);
        IVostokHerculesSinkBuilder SetClusterConfigClusterProvider([NotNull] string path);
        IVostokHerculesSinkBuilder SetServiceDiscoveryClusterProvider([NotNull] string environment, [NotNull] string application);

        IVostokHerculesSinkBuilder SuppressVerboseLogging();
    }
}