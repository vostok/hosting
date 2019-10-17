using System;
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
        private string application;
        private string environment;
        private ReplicaInfoSetup setup;
        private bool enabled;

        public ServiceBeaconBuilder()
        {
            setup = s => s
                .SetEnvironment(environment)
                .SetApplication(application);

            settingsCustomization = new Customization<ServiceBeaconSettings>();
        }

        public IServiceBeacon Build(BuildContext context)
        {
            if (!enabled)
            {
                context.Log.LogDisabled("ServiceBeacon");
                return new DevNullServiceBeacon();
            }

            var zooKeeperClient = context.ZooKeeperClient;

            if (zooKeeperClient == null)
            {
                context.Log.LogDisabled("ServiceBeacon", "disabled ZooKeeperClient");
                return new DevNullServiceBeacon();
            }

            application = context.ApplicationIdentity.Application;
            environment = context.ApplicationIdentity.Environment;

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

                    setup(s);
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

        public IVostokServiceBeaconBuilder SetupReplicaInfo(ReplicaInfoSetup newSetup)
        {
            var oldSetup = setup;

            setup = c =>
            {
                oldSetup(c);
                newSetup(c);
            };

            return this;
        }

        public IVostokServiceBeaconBuilder CustomizeSettings(Action<ServiceBeaconSettings> settingsCustomization)
        {
            this.settingsCustomization.AddCustomization(settingsCustomization);
            return this;
        }
    }
}