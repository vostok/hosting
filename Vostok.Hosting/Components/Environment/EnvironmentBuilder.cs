using System.Threading;
using Vostok.Hosting.Components.ApplicationIdentity;
using Vostok.Hosting.Components.Log;
using Vostok.Hosting.Components.ServiceBeacon;
using Vostok.Hosting.Setup;
// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Environment
{
    internal class EnvironmentBuilder : IEnvironmentBuilder
    {
        private CancellationToken shutdownToken;
        private readonly ApplicationIdentityBuilder applicationIdentityBuilder;
        private readonly LogBuilder logBuilder;

        public EnvironmentBuilder()
        {
            shutdownToken = CancellationToken.None;
            applicationIdentityBuilder = new ApplicationIdentityBuilder();
            logBuilder = new LogBuilder();
        }

        public static VostokHostingEnvironment Build(EnvironmentSetup environmentSetup)
        {
            var builder = new EnvironmentBuilder();
            environmentSetup?.Invoke(builder);
            return builder.Build();
        }

        public VostokHostingEnvironment Build()
        {
            var applicationIdentity = applicationIdentityBuilder.Build();
            var log = logBuilder.Build(applicationIdentity);

            return new VostokHostingEnvironment
            {
                ApplicationIdentity = applicationIdentity,
                Log = log,
                ServiceBeacon = new DevNullServiceBeacon()
            };
        }

        public IEnvironmentBuilder SetShutdownToken(CancellationToken shutdownToken)
        {
            this.shutdownToken = shutdownToken;
            return this;
        }

        public IEnvironmentBuilder SetupLog(LogSetup logSetup)
        {
            logSetup(logBuilder);
            return this;
        }

        public IEnvironmentBuilder SetupApplicationIdentity(ApplicationIdentitySetup applicationIdentitySetup)
        {
            applicationIdentitySetup(applicationIdentityBuilder);
            return this;
        }
    }
}