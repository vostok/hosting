using System;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Tracing;
using Vostok.Commons.Helpers;
using Vostok.Hercules.Client;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hosting.Components.ClusterProvider;
using Vostok.Hosting.Helpers;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Hercules
{
    internal class HerculesSinkBuilder : IVostokHerculesSinkBuilder, IBuilder<(IHerculesSink sink, bool external)>
    {
        private readonly Customization<HerculesSinkSettings> settingsCustomization;
        private volatile ClusterProviderBuilder clusterProviderBuilder;
        private volatile Func<string> apiKeyProvider;
        private volatile bool verboseLogging;
        private volatile bool enabled;
        private volatile IHerculesSink instance;

        public HerculesSinkBuilder()
        {
            settingsCustomization = new Customization<HerculesSinkSettings>();
        }

        public bool IsEnabled => enabled;

        public (IHerculesSink sink, bool external) Build(BuildContext context)
        {
            if (!enabled)
            {
                context.LogDisabled("HerculesSink");
                return (null, false);
            }

            if (instance != null)
                return (instance, true);

            var cluster = clusterProviderBuilder?.Build(context);
            if (cluster == null)
            {
                context.LogDisabled("HerculesSink", "unconfigured cluster provider");
                return (null, false);
            }

            var log = context.Log;
            if (!verboseLogging)
                log = log.WithMinimumLevel(LogLevel.Warn);

            log = log.DropEvents(evt => evt?.MessageTemplate != null && evt.MessageTemplate.Contains("put event to a disposed"));

            // Note(kungurtsev): allow null api key provider, streams can be configured later.
            var settings = new HerculesSinkSettings(cluster, apiKeyProvider ?? (() => null))
            {
                AdditionalSetup = setup =>
                {
                    setup.ClientApplicationName = context.ApplicationIdentity.FormatServiceName();
                    setup.SetupDistributedTracing(context.Tracer);
                }
            };

            settingsCustomization.Customize(settings);

            return (new HerculesSink(settings, log), false);
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

        public IVostokHerculesSinkBuilder UseInstance(IHerculesSink instance)
        {
            this.instance = instance;

            return this;
        }

        public IVostokHerculesSinkBuilder SetApiKeyProvider(Func<string> apiKeyProvider)
        {
            instance = null;
            
            this.apiKeyProvider = apiKeyProvider ?? throw new ArgumentNullException(nameof(apiKeyProvider));
            
            return this;
        }

        public IVostokHerculesSinkBuilder SetClusterConfigTopology(string path)
        {
            instance = null;
            
            clusterProviderBuilder = ClusterProviderBuilder.FromClusterConfig(path ?? throw new ArgumentNullException(nameof(path)));
            
            return this;
        }

        public IVostokHerculesSinkBuilder SetServiceDiscoveryTopology(string environment, string application)
        {
            instance = null;

            clusterProviderBuilder = ClusterProviderBuilder.FromServiceDiscovery(
                environment ?? throw new ArgumentNullException(nameof(environment)),
                application ?? throw new ArgumentNullException(nameof(application)));
            
            return this;
        }

        public IVostokHerculesSinkBuilder SetExternalUrlTopology(string url)
        {
            instance = null;
            
            clusterProviderBuilder = ClusterProviderBuilder.FromValue(new FixedClusterProvider(url ?? throw new ArgumentNullException(nameof(url))));
            
            return this;
        }

        public IVostokHerculesSinkBuilder SetClusterProvider(IClusterProvider clusterProvider)
        {
            instance = null;
            
            clusterProviderBuilder = ClusterProviderBuilder.FromValue(clusterProvider ?? throw new ArgumentNullException(nameof(clusterProvider)));
            
            return this;
        }

        public IVostokHerculesSinkBuilder EnableVerboseLogging()
        {
            instance = null;
            
            verboseLogging = true;
            
            return this;
        }

        public IVostokHerculesSinkBuilder DisableVerboseLogging()
        {
            instance = null;
            
            verboseLogging = false;
            
            return this;
        }

        public IVostokHerculesSinkBuilder CustomizeSettings(Action<HerculesSinkSettings> settingsCustomization)
        {
            instance = null;
            
            this.settingsCustomization.AddCustomization(settingsCustomization ?? throw new ArgumentNullException(nameof(settingsCustomization)));
            
            return this;
        }
    }
}