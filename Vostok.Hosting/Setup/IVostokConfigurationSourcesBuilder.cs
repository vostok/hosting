using System;
using JetBrains.Annotations;
using Vostok.ClusterConfig.Client.Abstractions;
using Vostok.Configuration.Abstractions;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokConfigurationSourcesBuilder
    {
        IVostokConfigurationSourcesBuilder AddSource([NotNull] IConfigurationSource source);
        IVostokConfigurationSourcesBuilder AddSource([NotNull] Func<IClusterConfigClient, IConfigurationSource> sourceProvider);
        IVostokConfigurationSourcesBuilder AddSecretSource([NotNull] IConfigurationSource source);
    }
}