using System;
using JetBrains.Annotations;
using Vostok.Configuration;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.Merging;
using Vostok.Configuration.Printing;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokConfigurationBuilder
    {
        IVostokConfigurationBuilder AddSource([NotNull] IConfigurationSource source);

        IVostokConfigurationBuilder SetupConfigurationProvider([NotNull] Action<IConfigurationSource, IConfigurationProvider> providerSetup);

        IVostokConfigurationBuilder CustomizeMergeSourcesSettings([NotNull] Action<SettingsMergeOptions> settingsCustomization);

        IVostokConfigurationBuilder CustomizeConfigurationProviderSettings([NotNull] Action<ConfigurationProviderSettings> settingsCustomization);

        IVostokConfigurationBuilder CustomizePrintSettings([NotNull] Action<PrintSettings> settingsCustomization);
    }
}