using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Setup;
using Vostok.ServiceDiscovery;
using Vostok.ServiceDiscovery.Abstractions;
using Vostok.ServiceDiscovery.Helpers;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.ServiceDiscovery
{
    internal class ServiceBeaconBuilder : IServiceBeaconBuilder, IBuilder<IServiceBeacon>
    {
        private string application;
        private string environment;

        private ReplicaInfoSetup setup;
        private IZooKeeperPathEscaper pathEscaper;

        public ServiceBeaconBuilder()
        {
            setup = s => s
                .SetEnvironment(environment)
                .SetApplication(application);
        }

        public IServiceBeacon Build(BuildContext context)
        {
            var zooKeeperClient = context.ZooKeeperClient;

            if (zooKeeperClient == null)
                return new DevNullServiceBeacon();

            application = context.ApplicationIdentity.Application;
            environment = context.ApplicationIdentity.Environment;

            var settings = new ServiceBeaconSettings();
            if (pathEscaper != null)
                settings.ZooKeeperNodesPathEscaper = pathEscaper;

            return new Vostok.ServiceDiscovery.ServiceBeacon(
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

        public IServiceBeaconBuilder SetupReplicaInfo(ReplicaInfoSetup newSetup)
        {
            var oldSetup = setup;

            setup = c =>
            {
                oldSetup(c);
                newSetup(c);
            };

            return this;
        }

        public IServiceBeaconBuilder SetZooKeeperPathEscaper(IZooKeeperPathEscaper pathEscaper)
        {
            this.pathEscaper = pathEscaper;
            return this;
        }
    }
}