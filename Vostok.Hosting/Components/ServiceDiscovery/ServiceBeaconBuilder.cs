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
        private string application;
        private string environment;
        private ReplicaInfoSetup setup;

        private readonly Customization<ServiceBeaconSettings> settingsCustomization;

        public ServiceBeaconBuilder()
        {
            setup = s => s
                .SetEnvironment(environment)
                .SetApplication(application);

            settingsCustomization = new Customization<ServiceBeaconSettings>();
        }

        public IServiceBeacon Build(BuildContext context)
        {
            var zooKeeperClient = context.ZooKeeperClient;

            if (zooKeeperClient == null)
                return new DevNullServiceBeacon();

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