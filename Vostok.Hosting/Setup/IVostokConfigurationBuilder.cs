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
        IVostokConfigurationBuilder AddSecretSource([NotNull] IConfigurationSource source);

        IVostokConfigurationBuilder CustomizeConfigurationContext([NotNull] Action<IVostokConfigurationContext> configurationContextCustomization);

        IVostokConfigurationBuilder CustomizeSettingsMerging([NotNull] Action<SettingsMergeOptions> settingsCustomization);
        IVostokConfigurationBuilder CustomizeSecretSettingsMerging([NotNull] Action<SettingsMergeOptions> settingsCustomization);

        IVostokConfigurationBuilder CustomizeConfigurationProvider([NotNull] Action<ConfigurationProviderSettings> settingsCustomization);

        IVostokConfigurationBuilder CustomizePrintSettings([NotNull] Action<PrintSettings> settingsCustomization);
    }
}