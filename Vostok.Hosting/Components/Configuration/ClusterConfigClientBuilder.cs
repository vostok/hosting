using System;
using Vostok.Clusterclient.Tracing;
using Vostok.ClusterConfig.Client;
using Vostok.ClusterConfig.Client.Abstractions;
using Vostok.Commons.Helpers;
using Vostok.Hosting.Setup;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Configuration
{
    internal class ClusterConfigClientBuilder : IVostokClusterConfigClientBuilder, IBuilder<(IClusterConfigClient client, bool external)>
    {
        private readonly Customization<ClusterConfigClientSettings> settingsCustomization;

        private volatile IClusterConfigClient instance;

        public ClusterConfigClientBuilder()
            => settingsCustomization = new Customization<ClusterConfigClientSettings>();

        public (IClusterConfigClient client, bool external) Build(BuildContext context)
        {
            if (instance != null)
                return (instance, true);

            var settings = new ClusterConfigClientSettings
            {
                Log = context.Log,
                AdditionalSetup = setup =>
                {
                    // Note(kungurtsev): do not fill setup.ClientApplicationName here, because build of context.ApplicationIdentity requires ClusterConfigClient.
                    setup.SetupDistributedTracing(context.Tracer);
                }
            };

            settingsCustomization.Customize(settings);

            if (!settings.EnableLocalSettings && !settings.EnableClusterSettings)
            {
                context.LogDisabled("ClusterConfigClient");
                return (new DisabledClusterConfigClient(), false);
            }

            return (new ClusterConfigClient(settings), false);
        }

        public IVostokClusterConfigClientBuilder UseInstance(IClusterConfigClient instance)
        {
            this.instance = instance;
            
            return this;
        }

        public IVostokClusterConfigClientBuilder CustomizeSettings(Action<ClusterConfigClientSettings> settingsCustomization)
        {
            this.settingsCustomization.AddCustomization(settingsCustomization ?? throw new ArgumentNullException(nameof(settingsCustomization)));

            instance = null;

            return this;
        }
    }
}
