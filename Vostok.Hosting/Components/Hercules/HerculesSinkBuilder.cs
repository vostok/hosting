using System;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Tracing;
using Vostok.Hercules.Client;
using Vostok.Hosting.Components.ClusterProvider;
using Vostok.Hosting.Helpers;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Hercules
{
    internal class HerculesSinkBuilder : IVostokHerculesSinkBuilder, IBuilder<HerculesSink>
    {
        private readonly Customization<HerculesSinkSettings> settingsCustomization;
        private volatile ClusterProviderBuilder clusterProviderBuilder;
        private volatile Func<string> apiKeyProvider;
        private volatile bool verboseLogging;
        private volatile bool enabled;

        public HerculesSinkBuilder()
        {
            settingsCustomization = new Customization<HerculesSinkSettings>();
        }

        public HerculesSink Build(BuildContext context)
        {
            if (!enabled)
            {
                context.LogDisabled("HerculesSink");
                return null;
            }

            var cluster = clusterProviderBuilder?.Build(context);
            if (cluster == null)
            {
                context.LogDisabled("HerculesSink", "unconfigured cluster provider");
                return null;
            }

            var log = context.Log;
            if (!verboseLogging)
                log = log.WithMinimumLevel(LogLevel.Warn);

            // Note(kungurtsev): allow null api key provider, streams can be configured later.
            var settings = new HerculesSinkSettings(cluster, apiKeyProvider ?? (() => null))
            {
                AdditionalSetup = setup => setup.SetupDistributedTracing(context.Tracer)
            };

            settingsCustomization.Customize(settings);

            return new HerculesSink(settings, log);
        }

        public IVostokHerculesSinkBuilder Enable()
        {
            enabled = true;
            return this;
        }

        public IVostokHerculesSinkBuilder Disable()
        {
            enabled = false;
            return this;
        }

        public IVostokHerculesSinkBuilder SetApiKeyProvider(Func<string> apiKeyProvider)
        {
            this.apiKeyProvider = apiKeyProvider;
            return this;
        }

        public IVostokHerculesSinkBuilder SetClusterConfigTopology(string path)
        {
            clusterProviderBuilder = ClusterProviderBuilder.FromClusterConfig(path);
            return this;
        }

        public IVostokHerculesSinkBuilder SetServiceDiscoveryTopology(string environment, string application)
        {
            clusterProviderBuilder = ClusterProviderBuilder.FromServiceDiscovery(environment, application);
            return this;
        }

        public IVostokHerculesSinkBuilder SetClusterProvider(IClusterProvider clusterProvider)
        {
            clusterProviderBuilder = ClusterProviderBuilder.FromValue(clusterProvider);
            return this;
        }

        public IVostokHerculesSinkBuilder EnableVerboseLogging()
        {
            verboseLogging = true;
            return this;
        }

        public IVostokHerculesSinkBuilder DisableVerboseLogging()
        {
            verboseLogging = false;
            return this;
        }

        public IVostokHerculesSinkBuilder CustomizeSettings(Action<HerculesSinkSettings> settingsCustomization)
        {
            this.settingsCustomization.AddCustomization(settingsCustomization);
            return this;
        }
    }
}