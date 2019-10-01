using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Tracing;
using Vostok.Hercules.Client;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hosting.Components.ClusterProvider;
using Vostok.Hosting.Components.String;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Hercules
{
    internal class HerculesSinkBuilder : IHerculesSinkBuilder, IBuilder<IHerculesSink>
    {
        private ClusterProviderBuilder clusterProviderBuilder;
        private StringProviderBuilder apiKeyProviderBuilder;
        private bool suppressVerboseLogging;

        [NotNull]
        public IHerculesSink Build(BuildContext context)
        {
            var cluster = clusterProviderBuilder?.Build(context);
            if (cluster == null)
                return new DevNullHerculesSink();

            var apiKeyProvider = apiKeyProviderBuilder?.Build(context) ?? (() => null);

            var log = context.Log;
            if (suppressVerboseLogging)
                log = log.WithMinimumLevel(LogLevel.Warn);

            return new HerculesSink(new HerculesSinkSettings(cluster, apiKeyProvider)
            {
                AdditionalSetup = setup => setup.SetupDistributedTracing(context.Tracer)
            }, log);
        }
        
        public IHerculesSinkBuilder SetApiKeyProvider(Func<string> apiKeyProvider)
        {
            apiKeyProviderBuilder = StringProviderBuilder.FromValueProvider(apiKeyProvider);
            return this;
        }

        public IHerculesSinkBuilder SetClusterConfigApiKeyProvider(string path)
        {
            apiKeyProviderBuilder = StringProviderBuilder.FromClusterConfig(path);
            return this;
        }

        public IHerculesSinkBuilder SetClusterConfigClusterProvider(string path)
        {
            clusterProviderBuilder = ClusterProviderBuilder.FromClusterConfig(path);
            return this;
        }

        public IHerculesSinkBuilder SetServiceDiscoveryClusterProvider(string environment, string application)
        {
            clusterProviderBuilder = ClusterProviderBuilder.FromServiceDiscovery(environment, application);
            return this;
        }

        public IHerculesSinkBuilder SetClusterProvider(IClusterProvider clusterProvider)
        {
            clusterProviderBuilder = ClusterProviderBuilder.FromValue(clusterProvider);
            return this;
        }

        public IHerculesSinkBuilder SuppressVerboseLogging()
        {
            suppressVerboseLogging = true;
            return this;
        }
    }
}