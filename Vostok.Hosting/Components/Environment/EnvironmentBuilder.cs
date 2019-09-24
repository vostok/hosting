using System;
using Vostok.Hosting.Components.ApplicationIdentity;
using Vostok.Hosting.Components.Configuration;
using Vostok.Hosting.Components.Hercules;
using Vostok.Hosting.Components.Log;
using Vostok.Hosting.Components.ServiceBeacon;
using Vostok.Hosting.Setup;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Environment
{
    internal class EnvironmentBuilder : IEnvironmentBuilder, IDisposable
    {
        private readonly ApplicationIdentityBuilder applicationIdentityBuilder;
        private readonly CompositeLogBuilder compositeLogBuilder;
        private readonly HerculesSinkBuilder herculesSinkBuilder;
        private readonly ClusterConfigClientBuilder clusterConfigClientBuilder;

        public EnvironmentBuilder()
        {
            applicationIdentityBuilder = new ApplicationIdentityBuilder();
            compositeLogBuilder = new CompositeLogBuilder();
            herculesSinkBuilder = new HerculesSinkBuilder();
            clusterConfigClientBuilder = new ClusterConfigClientBuilder();
        }

        public VostokHostingEnvironment Build()
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var context = new Context();

            context.ApplicationIdentity = applicationIdentityBuilder.Build(context);
            context.Log = compositeLogBuilder.Build(context);

            context.ClusterConfigClient = clusterConfigClientBuilder.Build(context);

            context.HerculesSink = herculesSinkBuilder.Build(context);
            context.Log = compositeLogBuilder.Build(context);

            return new VostokHostingEnvironment
            {
                ApplicationIdentity = context.ApplicationIdentity,
                Log = compositeLogBuilder.Build(context), //without context SubstitutableLog
                ServiceBeacon = new DevNullServiceBeacon(),
                HerculesSink = context.HerculesSink
            };
        }

        public IEnvironmentBuilder SetupLog(EnvironmentSetup<ICompositeLogBuilder> compositeLogSetup)
        {
            compositeLogSetup(compositeLogBuilder);
            return this;
        }

        public IEnvironmentBuilder SetupClusterConfigClient(EnvironmentSetup<IClusterConfigClientBuilder> clusterConfigClientSetup)
        {
            clusterConfigClientSetup(clusterConfigClientBuilder);
            return this;
        }

        public IEnvironmentBuilder SetupApplicationIdentity(EnvironmentSetup<IApplicationIdentityBuilder> applicationIdentitySetup)
        {
            applicationIdentitySetup(applicationIdentityBuilder);
            return this;
        }

        public IEnvironmentBuilder SetupHerculesSink(EnvironmentSetup<IHerculesSinkBuilder> sinkSetup)
        {
            sinkSetup(herculesSinkBuilder);
            return this;
        }

        public void Dispose()
        {
        }
    }
}