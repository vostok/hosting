using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Tracing;
using Vostok.Hercules.Client;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hosting.Components.ClusterProvider;
using Vostok.Hosting.Components.String;
using Vostok.Hosting.Helpers;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Hercules
{
    internal class HerculesSinkBuilder : IVostokHerculesSinkBuilder, IBuilder<IHerculesSink>
    {
        private ClusterProviderBuilder clusterProviderBuilder;
        private StringProviderBuilder apiKeyProviderBuilder;
        private bool suppressVerboseLogging;
        private readonly SettingsCustomization<HerculesSinkSettings> settingsCustomization;

        public HerculesSinkBuilder()
        {
            settingsCustomization = new SettingsCustomization<HerculesSinkSettings>();
        }

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

            var settings = new HerculesSinkSettings(cluster, apiKeyProvider)
            {
                AdditionalSetup = setup => setup.SetupDistributedTracing(context.Tracer)
            };

            settingsCustomization.Customize(settings);

            return new HerculesSink(settings, log);
        }
        
        public IVostokHerculesSinkBuilder SetApiKeyProvider(Func<string> apiKeyProvider)
        {
            apiKeyProviderBuilder = StringProviderBuilder.FromValueProvider(apiKeyProvider);
            return this;
        }

        public IVostokHerculesSinkBuilder SetClusterConfigApiKeyProvider(string path)
        {
            apiKeyProviderBuilder = StringProviderBuilder.FromClusterConfig(path);
            return this;
        }

        public IVostokHerculesSinkBuilder SetClusterConfigClusterProvider(string path)
        {
            clusterProviderBuilder = ClusterProviderBuilder.FromClusterConfig(path);
            return this;
        }

        public IVostokHerculesSinkBuilder SetServiceDiscoveryClusterProvider(string environment, string application)
        {
            clusterProviderBuilder = ClusterProviderBuilder.FromServiceDiscovery(environment, application);
            return this;
        }

        public IVostokHerculesSinkBuilder SetClusterProvider(IClusterProvider clusterProvider)
        {
            clusterProviderBuilder = ClusterProviderBuilder.FromValue(clusterProvider);
            return this;
        }

        public IVostokHerculesSinkBuilder SuppressVerboseLogging()
        {
            suppressVerboseLogging = true;
            return this;
        }

        public IVostokHerculesSinkBuilder CustomizeSettings(Action<HerculesSinkSettings> settingsCustomization)
        {
            this.settingsCustomization.AddCustomization(settingsCustomization);
            return this;
        }
    }
}