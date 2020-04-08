using System;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Models;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.Components.Environment
{
    internal class ConfigurationSetupContext : IVostokConfigurationSetupContext
    {
        private readonly Func<IVostokApplicationIdentity> identityProvider;

        public ConfigurationSetupContext(ILog log, Func<IVostokApplicationIdentity> identityProvider)
        {
            Log = log.ForContext<VostokHostingEnvironment>();

            this.identityProvider = identityProvider;
        }

        public ILog Log { get; }

        public IVostokApplicationIdentity ApplicationIdentity => identityProvider();
    }
}
