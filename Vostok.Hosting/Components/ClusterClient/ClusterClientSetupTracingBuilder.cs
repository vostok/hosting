using System;
using Vostok.Clusterclient.Tracing;
using Vostok.Hosting.Helpers;
using Vostok.Hosting.Setup;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.ClusterClient
{
    internal class ClusterClientSetupTracingBuilder : IVostokClusterClientSetupTracingBuilder, IBuilder<TracingConfiguration>
    {
        private readonly Customization<TracingConfiguration> settingsCustomization;

        public ClusterClientSetupTracingBuilder()
        {
            settingsCustomization = new Customization<TracingConfiguration>();
        }

        public TracingConfiguration Build(BuildContext context)
        {
            var settings = new TracingConfiguration(context.Tracer);

            settingsCustomization.Customize(settings);

            return settings;
        }

        public IVostokClusterClientSetupTracingBuilder CustomizeSettings(Action<TracingConfiguration> settingsCustomization)
        {
            this.settingsCustomization.AddCustomization(settingsCustomization);
            return this;
        }
    }
}