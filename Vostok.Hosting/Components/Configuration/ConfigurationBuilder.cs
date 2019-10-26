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

        private readonly Customization<SettingsMergeOptions> mergeSettingsCustomization;
        private readonly Customization<ConfigurationProviderSettings> configurationSettingsCustomization;
        private readonly Customization<PrintSettings> printSettingsCustomization;
        private readonly Customization<IVostokConfigurationContext> configurationContextCustomization;

        public ConfigurationBuilder()
        {
            sources = new List<IConfigurationSource>();
            mergeSettingsCustomization = new Customization<SettingsMergeOptions>();
            configurationSettingsCustomization = new Customization<ConfigurationProviderSettings>();
            printSettingsCustomization = new Customization<PrintSettings>();
            configurationContextCustomization = new Customization<IVostokConfigurationContext>();
        }

        public IVostokConfigurationBuilder AddSource(IConfigurationSource source)
        {
            sources.Add(source);
            return this;
        }

        public IVostokConfigurationBuilder CustomizeSettingsMerging(Action<SettingsMergeOptions> settingsCustomization)
        {
            mergeSettingsCustomization.AddCustomization(settingsCustomization);
            return this;
        }

        public IVostokConfigurationBuilder CustomizeConfigurationProvider(Action<ConfigurationProviderSettings> settingsCustomization)
        {
            configurationSettingsCustomization.AddCustomization(settingsCustomization);
            return this;
        }

        public IVostokConfigurationBuilder CustomizePrintSettings(Action<PrintSettings> settingsCustomization)
        {
            printSettingsCustomization.AddCustomization(settingsCustomization);
            return this;
        }

        public IVostokConfigurationBuilder CustomizeConfigurationContext(Action<IVostokConfigurationContext> configurationContextCustomization)
        {
            this.configurationContextCustomization.AddCustomization(configurationContextCustomization);
            return this;
        }

        public (IConfigurationSource, IConfigurationProvider) Build(BuildContext context)
        {
            var mergeOptions = new SettingsMergeOptions();
            mergeSettingsCustomization.Customize(mergeOptions);

            var source = sources.Any()
                ? (IConfigurationSource)new CombinedSource(sources.ToArray(), mergeOptions)
                : new ConstantSource(null);

            var printSettings = new PrintSettings();
            printSettingsCustomization.Customize(printSettings);

            var providerSettings =
                new ConfigurationProviderSettings()
                    .WithErrorLogging(context.Log)
                    .WithSettingsLogging(context.Log, printSettings);
            configurationSettingsCustomization.Customize(providerSettings);

            var provider = new ConfigurationProvider(providerSettings);

            configurationContextCustomization.Customize(new ConfigurationContext(source, provider, context.ClusterConfigClient));

            return (source, provider);
        }
    }
}