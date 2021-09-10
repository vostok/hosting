using System;
using Vostok.Datacenters;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Models;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.Components.Environment
{
    internal class ConfigurationSetupContext : IVostokConfigurationSetupContext
    {
        private readonly Func<IVostokApplicationIdentity> identityProvider;

        public ConfigurationSetupContext(ILog log, IDatacenters datacenters, Func<IVostokApplicationIdentity> identityProvider)
        {
            Log = log.ForContext<VostokHostingEnvironment>();
            Datacenters = datacenters;

            this.identityProvider = identityProvider;
        }

        public ILog Log { get; }

        public IVostokApplicationIdentity ApplicationIdentity => identityProvider();

        public IDatacenters Datacenters { get; }
    }
}