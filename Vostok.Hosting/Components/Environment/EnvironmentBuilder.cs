using System;
using Vostok.Hosting.Components.ApplicationIdentity;
using Vostok.Hosting.Components.Configuration;
using Vostok.Hosting.Components.Hercules;
using Vostok.Hosting.Components.Log;
using Vostok.Hosting.Components.ServiceBeacon;
using Vostok.Hosting.Components.Tracing;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.Tracing.Abstractions;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Environment
{
    internal class EnvironmentBuilder : IEnvironmentBuilder, IDisposable
    {
        private readonly ApplicationIdentityBuilder applicationIdentityBuilder;
        private readonly CompositeLogBuilder compositeLogBuilder;
        private readonly HerculesSinkBuilder herculesSinkBuilder;
        private readonly ClusterConfigClientBuilder clusterConfigClientBuilder;
        private readonly TracerBuilder tracerBuilder;

        public EnvironmentBuilder()
        {
            applicationIdentityBuilder = new ApplicationIdentityBuilder();
            compositeLogBuilder = new CompositeLogBuilder();
            herculesSinkBuilder = new HerculesSinkBuilder();
            clusterConfigClientBuilder = new ClusterConfigClientBuilder();
            tracerBuilder = new TracerBuilder();
        }

        public VostokHostingEnvironment Build()
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var context = new BuildContext();

            LogProvider.Configure(context.Log, true);
            TracerProvider.Configure(context.Tracer, true);

            context.ApplicationIdentity = applicationIdentityBuilder.Build(context);
            Substitute(context);

            context.ClusterConfigClient = clusterConfigClientBuilder.Build(context);
            Substitute(context);
            
            context.ApplicationIdentity = applicationIdentityBuilder.Build(context);
            Substitute(context);
            
            context.HerculesSink = herculesSinkBuilder.Build(context);
            Substitute(context);

            var finalLog = compositeLogBuilder.Build(context);
            LogProvider.Configure(finalLog, true);
            var finalTracer = tracerBuilder.Build(context);
            TracerProvider.Configure(finalTracer, true);

            return new VostokHostingEnvironment
            {
                ApplicationIdentity = context.ApplicationIdentity,
                Log = finalLog,
                Tracer = finalTracer,
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

        public IEnvironmentBuilder SetupTracer(EnvironmentSetup<ITracerBuilder> tracerSetup)
        {
            tracerSetup(tracerBuilder);
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
        
        private void Substitute(BuildContext context)
        {
            // Note(kungurtsev): requires hercules, not disposable and lightweight.
            // Note(kungurtsev): get returns same instance (with substitutable base), so can be executed in any order.
            context.Log = compositeLogBuilder.Build(context);
            context.Tracer = tracerBuilder.Build(context);
        }
    }
}