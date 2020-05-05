using System;
using JetBrains.Annotations;
using Vostok.ClusterConfig.Client;
using Vostok.ClusterConfig.Client.Abstractions;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokClusterConfigClientBuilder
    {
        IVostokClusterConfigClientBuilder UseInstance([NotNull] IClusterConfigClient instance);

        IVostokClusterConfigClientBuilder CustomizeSettings([NotNull] Action<ClusterConfigClientSettings> settingsCustomization);
    }
}