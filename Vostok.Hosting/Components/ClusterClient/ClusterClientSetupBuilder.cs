using System;
using Vostok.Clusterclient.Context;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Ordering.Weighed;
using Vostok.ClusterClient.Datacenters;
using Vostok.Clusterclient.Tracing;
using Vostok.Commons.Helpers;
using Vostok.Hosting.Setup;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.ClusterClient
{
    internal class ClusterClientSetupBuilder : IVostokClusterClientSetupBuilder, IBuilder<ClusterClientSetup>
    {
        private ClusterClientSetupTracingBuilder tracingBuilder;
        private Customization<IClusterClientConfiguration> settingsCustomization;
        private Customization<IWeighedReplicaOrderingBuilder> weightOrderingCustomization;

        public ClusterClientSetupBuilder()
        {
            tracingBuilder = new ClusterClientSetupTracingBuilder();
            settingsCustomization = new Customization<IClusterClientConfiguration>();
            weightOrderingCustomization = new Customization<IWeighedReplicaOrderingBuilder>();
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
                            weightOrderingCustomization.Customize(weightOrdering);
                        });

                settingsCustomization.Customize(c);
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
            this.settingsCustomization.AddCustomization(settingsCustomization ?? throw new ArgumentNullException(nameof(settingsCustomization)));
            return this;
        }

        public IVostokClusterClientSetupBuilder CustomizeWeightOrdering(Action<IWeighedReplicaOrderingBuilder> weightOrderingCustomization)
        {
            this.weightOrderingCustomization.AddCustomization(weightOrderingCustomization ?? throw new ArgumentNullException(nameof(weightOrderingCustomization)));
            return this;
        }
    }
}