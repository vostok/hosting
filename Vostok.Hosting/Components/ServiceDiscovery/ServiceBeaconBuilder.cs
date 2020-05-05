using System;
using Vostok.Commons.Helpers;
using Vostok.Datacenters;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Components.ZooKeeper;
using Vostok.Hosting.Helpers;
using Vostok.Hosting.Setup;
using Vostok.ServiceDiscovery;
using Vostok.ServiceDiscovery.Abstractions;
using Vostok.ZooKeeper.Client.Abstractions;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.ServiceDiscovery
{
    internal class ServiceBeaconBuilder : IVostokServiceBeaconBuilder, IBuilder<IServiceBeacon>
    {
        private readonly Customization<ServiceBeaconSettings> settingsCustomization;
        private readonly Customization<IReplicaInfoBuilder> replicaInfoCustomization;
        private volatile IVostokApplicationIdentity applicationIdentity;
        private volatile bool enabled;
        private volatile bool registrationDeniedFromNonActiveDatacenters;

        public ServiceBeaconBuilder()
        {
            replicaInfoCustomization = new Customization<IReplicaInfoBuilder>();

            replicaInfoCustomization.AddCustomization(
                s =>
                {
                    s.SetEnvironment(applicationIdentity.Environment);
                    s.SetApplication(applicationIdentity.FormatServiceName());
                });

            settingsCustomization = new Customization<ServiceBeaconSettings>();
        }

        public bool IsEnabled => enabled;

        public IServiceBeacon Build(BuildContext context)
        {
            applicationIdentity = context.ApplicationIdentity;

            if (!enabled)
            {
                context.LogDisabled("ServiceBeacon");
                return new DevNullServiceBeacon(CreateReplicaInfo(context));
            }

            var zooKeeperClient = context.ZooKeeperClient;
            if (zooKeeperClient == null)
            {
                context.LogDisabled("ServiceBeacon", "disabled ZooKeeperClient");
                return new DevNullServiceBeacon(CreateReplicaInfo(context));
            }

            return CreateBeacon(zooKeeperClient, context);
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

        public IVostokServiceBeaconBuilder DenyRegistrationFromNotActiveDatacenters()
        {
            registrationDeniedFromNonActiveDatacenters = true;
            return this;
        }

        public IVostokServiceBeaconBuilder AllowRegistrationFromNotActiveDatacenters()
        {
            registrationDeniedFromNonActiveDatacenters = false;
            return this;
        }

        public IVostokServiceBeaconBuilder SetupReplicaInfo(ReplicaInfoSetup setup)
        {
            setup = setup ?? throw new ArgumentNullException(nameof(setup));
            replicaInfoCustomization.AddCustomization(c => setup(c));
            return this;
        }

        public IVostokServiceBeaconBuilder CustomizeSettings(Action<ServiceBeaconSettings> settingsCustomization)
        {
            this.settingsCustomization.AddCustomization(settingsCustomization ?? throw new ArgumentNullException(nameof(settingsCustomization)));
            return this;
        }

        private Func<bool> LocalDatacenterIsActive(IDatacenters datacenters)
        {
            if (datacenters == null)
                return null;

            return datacenters.LocalDatacenterIsActive;
        }

        private IReplicaInfo CreateReplicaInfo(BuildContext context)
            => CreateBeacon(new DevNullZooKeeperClient(), context).ReplicaInfo;

        private ServiceBeacon CreateBeacon(IZooKeeperClient zooKeeperClient, BuildContext context)
        {
            var settings = new ServiceBeaconSettings();

            if (registrationDeniedFromNonActiveDatacenters)
                settings.RegistrationAllowedProvider = LocalDatacenterIsActive(context.Datacenters);

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
    }
}
