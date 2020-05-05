using System;
using JetBrains.Annotations;
using Vostok.Configuration;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.Merging;
using Vostok.Configuration.Printing;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokConfigurationBuilder : IVostokConfigurationSourcesBuilder
    {
        IVostokConfigurationBuilder CustomizeConfigurationContext([NotNull] Action<IVostokConfigurationContext> configurationContextCustomization);

        IVostokConfigurationBuilder CustomizeSettingsMerging([NotNull] Action<SettingsMergeOptions> settingsCustomization);
        IVostokConfigurationBuilder CustomizeSecretSettingsMerging([NotNull] Action<SettingsMergeOptions> settingsCustomization);

        IVostokConfigurationBuilder CustomizeConfigurationSource([NotNull] Func<IConfigurationSource, IConfigurationSource> customization);
        IVostokConfigurationBuilder CustomizeSecretConfigurationSource([NotNull] Func<IConfigurationSource, IConfigurationSource> customization);

        IVostokConfigurationBuilder CustomizeConfigurationProvider([NotNull] Action<ConfigurationProviderSettings> settingsCustomization);
        IVostokConfigurationBuilder CustomizeSecretConfigurationProvider([NotNull] Action<ConfigurationProviderSettings> settingsCustomization);

        IVostokConfigurationBuilder CustomizePrintSettings([NotNull] Action<PrintSettings> settingsCustomization);
    }
}