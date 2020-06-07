using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Extensions;
using Vostok.Hosting.Abstractions.Diagnostics;

namespace Vostok.Hosting.Components.Diagnostics.InfoProviders
{
    internal class ConfigurationInfoProvider : IDiagnosticInfoProvider
    {
        private readonly IConfigurationSource source;

        public ConfigurationInfoProvider(IConfigurationSource source)
            => this.source = source;

        public object Query() => source.Get();
    }
}
