using System;
using System.Text;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Helpers;
using Vostok.Hosting.Setup;
using Vostok.ServiceDiscovery;
using Vostok.ServiceDiscovery.Abstractions;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.ServiceDiscovery
{
    internal class ServiceBeaconBuilder : IVostokServiceBeaconBuilder, IBuilder<IServiceBeacon>
    {
        private readonly Customization<ServiceBeaconSettings> settingsCustomization;
        private readonly Customization<IReplicaInfoBuilder> replicaInfoCustomization;
        private volatile IVostokApplicationIdentity applicationIdentity;
        private volatile bool enabled;

        public ServiceBeaconBuilder()
        {
            replicaInfoCustomization = new Customization<IReplicaInfoBuilder>();
            replicaInfoCustomization.AddCustomization(
                s => s
                    .SetEnvironment(applicationIdentity.Environment)
                    .SetApplication(FormatApplication(applicationIdentity)));

            settingsCustomization = new Customization<ServiceBeaconSettings>();
        }

        public IServiceBeacon Build(BuildContext context)
        {
            if (!enabled)
            {
                context.LogDisabled("ServiceBeacon");
                return new DevNullServiceBeacon();
            }

            var zooKeeperClient = context.ZooKeeperClient;
            if (zooKeeperClient == null)
            {
                context.LogDisabled("ServiceBeacon", "disabled ZooKeeperClient");
                return new DevNullServiceBeacon();
            }

            applicationIdentity = context.ApplicationIdentity;

            var settings = new ServiceBeaconSettings();

            settingsCustomization.Customize(settings);

            return new ServiceBeacon(
                zooKeeperClient,
                s =>
                {
                    s.SetProperty(WellKnownApplicationIdentityProperties.Project, context.ApplicationIdentity.Project);
                    s.SetProperty(WellKnownApplicationIdentityProperties.Subproject, context.ApplicationIdentity.Subproject);
                    s.SetProperty(WellKnownApplicationIdentityProperties.Environment, context.ApplicationIdentity.Environment);
                    s.SetProperty(WellKnownApplicationIdentityProperties.Application, context.ApplicationIdentity.Application);
                    s.SetProperty(WellKnownApplicationIdentityProperties.Instance, context.ApplicationIdentity.Instance);

                    replicaInfoCustomization.Customize(s);
                },
                settings,
                context.Log);
        }

        public IVostokServiceBeaconBuilder Enable()
        {
            enabled = true;
            return this;
        }

        public IVostokServiceBeaconBuilder Disable()
        {
            enabled = false;
            return this;
        }

        public IVostokServiceBeaconBuilder SetupReplicaInfo(ReplicaInfoSetup setup)
        {
            replicaInfoCustomization.AddCustomization(c => setup(c));
            return this;
        }

        public IVostokServiceBeaconBuilder CustomizeSettings(Action<ServiceBeaconSettings> settingsCustomization)
        {
            this.settingsCustomization.AddCustomization(settingsCustomization);
            return this;
        }

        private static string FormatApplication(IVostokApplicationIdentity identity)
        {
            var result = new StringBuilder();

            result.Append(identity.Project);

            if (identity.Subproject != null)
            {
                result.Append(".");
                result.Append(identity.Subproject);
            }

            result.Append(".");
            result.Append(identity.Application);

            return result.ToString();
        }
    }
}