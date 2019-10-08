using System;
using JetBrains.Annotations;
using Vostok.ClusterConfig.Client;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokClusterConfigClientBuilder
    {
        IVostokClusterConfigClientBuilder CustomizeSettings([NotNull] Action<ClusterConfigClientSettings> settingsCustomization);
    }
}