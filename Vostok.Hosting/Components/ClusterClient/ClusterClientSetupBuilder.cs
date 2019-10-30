using System;
using Vostok.Clusterclient.Context;
using Vostok.Clusterclient.Core;
using Vostok.ClusterClient.Datacenters;
using Vostok.Clusterclient.Tracing;
using Vostok.Hosting.Helpers;
using Vostok.Hosting.Setup;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.ClusterClient
{
    internal class ClusterClientSetupBuilder : IVostokClusterClientSetupBuilder, IBuilder<ClusterClientSetup>
    {
        private ClusterClientSetupTracingBuilder tracingBuilder;
        private Customization<IClusterClientConfiguration> customization;

        public ClusterClientSetupBuilder()
        {
            tracingBuilder = new ClusterClientSetupTracingBuilder();
            customization = new Customization<IClusterClientConfiguration>();
        }

        public ClusterClientSetup Build(BuildContext context)
        {
            void Setup(IClusterClientConfiguration c)
            {
                c.SetupDistributedContext();
                c.SetupDistributedTracing(tracingBuilder.Build(context));

                if (context.Datacenters != null)
                    c.SetupWeighedReplicaOrdering(
                        weightOrdering =>
                        {
                            weightOrdering.SetupAvoidInactiveDatacentersWeightModifier(context.Datacenters);
                            weightOrdering.SetupBoostLocalDatacentersWeightModifier(context.Datacenters);
                        });

                customization.Customize(c);
            }

            return Setup;
        }

        public IVostokClusterClientSetupBuilder SetupTracing(Action<IVostokClusterClientSetupTracingBuilder> tracingSetup)
        {
            tracingSetup = tracingSetup ?? throw new ArgumentNullException(nameof(tracingSetup));
            tracingSetup(tracingBuilder);
            return this;
        }

        public IVostokClusterClientSetupBuilder CustomizeSettings(Action<IClusterClientConfiguration> settingsCustomization)
        {
            customization.AddCustomization(settingsCustomization ?? throw new ArgumentNullException(nameof(settingsCustomization)));
            return this;
        }
    }
}