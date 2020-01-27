using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Tracing;
using Vostok.ClusterConfig.Client;
using Vostok.Commons.Helpers;
using Vostok.Hosting.Setup;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Configuration
{
    internal class ClusterConfigClientBuilder : IVostokClusterConfigClientBuilder, IBuilder<ClusterConfigClient>
    {
        private readonly Customization<ClusterConfigClientSettings> settingsCustomization;

        public ClusterConfigClientBuilder()
            => settingsCustomization = new Customization<ClusterConfigClientSettings>();

        [NotNull]
        public ClusterConfigClient Build(BuildContext context)
        {
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

            return new ClusterConfigClient(settings);
        }

        public IVostokClusterConfigClientBuilder CustomizeSettings(Action<ClusterConfigClientSettings> settingsCustomization)
        {
            this.settingsCustomization.AddCustomization(settingsCustomization ?? throw new ArgumentNullException(nameof(settingsCustomization)));
            return this;
        }
    }
}