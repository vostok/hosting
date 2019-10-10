using System;
using System.Collections.Generic;
using System.Linq;
using Vostok.Configuration;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.Merging;
using Vostok.Configuration.Logging;
using Vostok.Configuration.Printing;
using Vostok.Configuration.Sources.Combined;
using Vostok.Configuration.Sources.Constant;
using Vostok.Hosting.Helpers;
using Vostok.Hosting.Setup;
// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Configuration
{
    internal class ConfigurationBuilder : IVostokConfigurationBuilder, IBuilder<(IConfigurationSource, IConfigurationProvider)>
    {
        private readonly List<IConfigurationSource> sources;
        private readonly List<Action<IConfigurationSource, IConfigurationProvider>> configurationProviderSetups;

        private readonly SettingsCustomization<SettingsMergeOptions> mergeSettingsCustomization;
        private readonly SettingsCustomization<ConfigurationProviderSettings> configurationSettingsCustomization;
        private readonly SettingsCustomization<PrintSettings> printSettingsCustomization;

        public ConfigurationBuilder()
        {
            sources = new List<IConfigurationSource>();
            configurationProviderSetups = new List<Action<IConfigurationSource, IConfigurationProvider>>();
            mergeSettingsCustomization = new SettingsCustomization<SettingsMergeOptions>();
            configurationSettingsCustomization = new SettingsCustomization<ConfigurationProviderSettings>();
            printSettingsCustomization = new SettingsCustomization<PrintSettings>();
        }

        public IVostokConfigurationBuilder AddSource(IConfigurationSource source)
        {
            sources.Add(source);
            return this;
        }

        public IVostokConfigurationBuilder CustomizeMergeSourcesSettings(Action<SettingsMergeOptions> settingsCustomization)
        {
            mergeSettingsCustomization.AddCustomization(settingsCustomization);
            return this;
        }

        public IVostokConfigurationBuilder CustomizeConfigurationProviderSettings(Action<ConfigurationProviderSettings> settingsCustomization)
        {
            configurationSettingsCustomization.AddCustomization(settingsCustomization);
            return this;
        }

        public IVostokConfigurationBuilder CustomizePrintSettings(Action<PrintSettings> settingsCustomization)
        {
            printSettingsCustomization.AddCustomization(settingsCustomization);
            return this;
        }

        public IVostokConfigurationBuilder SetupConfigurationProvider(Action<IConfigurationSource, IConfigurationProvider> providerSetup)
        {
            configurationProviderSetups.Add(providerSetup);
            return this;
        }

        public (IConfigurationSource, IConfigurationProvider) Build(BuildContext context)
        {
            var mergeSettings = new SettingsMergeOptions();
            mergeSettingsCustomization.Customize(mergeSettings);

            var source = sources.Any()
                ? (IConfigurationSource)new CombinedSource(sources.ToArray(), mergeSettings)
                : new ConstantSource(null);

            var printSettings = new PrintSettings();
            printSettingsCustomization.Customize(printSettings);

            var providerSettings = 
                new ConfigurationProviderSettings()
                    .WithErrorLogging(context.Log)
                    .WithSettingsLogging(context.Log, printSettings);
            configurationSettingsCustomization.Customize(providerSettings);

            var provider = new ConfigurationProvider(providerSettings);

            foreach (var setup in configurationProviderSetups)
                setup(source, provider);

            return (source, provider);
        }
    }
}