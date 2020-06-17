using System;
using System.Collections.Generic;
using System.Linq;
using Vostok.ClusterConfig.Client.Abstractions;
using Vostok.Commons.Helpers;
using Vostok.Configuration;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.Merging;
using Vostok.Configuration.Binders;
using Vostok.Configuration.Logging;
using Vostok.Configuration.Printing;
using Vostok.Configuration.Sources.Combined;
using Vostok.Configuration.Sources.Constant;
using Vostok.Configuration.Sources.Switching;
using Vostok.Hosting.Setup;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Configuration
{
    internal class ConfigurationBuilder : IVostokConfigurationBuilder,
        IBuilder<(SwitchingSource source, SwitchingSource secretSource, ConfigurationProvider provider, ConfigurationProvider secretProvider)>
    {
        private readonly List<IConfigurationSource> sources;
        private readonly List<IConfigurationSource> secretSources;
        private readonly List<Func<IClusterConfigClient, IConfigurationSource>> clusterConfigSources;

        private readonly Customization<SettingsMergeOptions> mergeSettingsCustomization;
        private readonly Customization<SettingsMergeOptions> mergeSecretSettingsCustomization;
        private readonly Customization<ConfigurationProviderSettings> providerCustomization;
        private readonly Customization<ConfigurationProviderSettings> secretProviderCustomization;
        private readonly Customization<PrintSettings> printSettingsCustomization;
        private readonly Customization<IVostokConfigurationContext> configurationContextCustomization;
        private readonly Customization<IConfigurationSource> sourceCustomization;
        private readonly Customization<IConfigurationSource> secretSourceCustomization;

        public ConfigurationBuilder()
        {
            sources = new List<IConfigurationSource>();
            secretSources = new List<IConfigurationSource>();
            clusterConfigSources = new List<Func<IClusterConfigClient, IConfigurationSource>>();
            mergeSettingsCustomization = new Customization<SettingsMergeOptions>();
            mergeSecretSettingsCustomization = new Customization<SettingsMergeOptions>();
            providerCustomization = new Customization<ConfigurationProviderSettings>();
            secretProviderCustomization = new Customization<ConfigurationProviderSettings>();
            printSettingsCustomization = new Customization<PrintSettings>();
            configurationContextCustomization = new Customization<IVostokConfigurationContext>();
            sourceCustomization = new Customization<IConfigurationSource>();
            secretSourceCustomization = new Customization<IConfigurationSource>();
        }

        public IVostokConfigurationSourcesBuilder AddSource(IConfigurationSource source)
        {
            sources.Add(source ?? throw new ArgumentNullException(nameof(source)));
            return this;
        }

        public IVostokConfigurationSourcesBuilder AddSource(Func<IClusterConfigClient, IConfigurationSource> sourceProvider)
        {
            clusterConfigSources.Add(sourceProvider ?? throw new ArgumentNullException(nameof(sourceProvider)));
            return this;
        }

        public IVostokConfigurationSourcesBuilder AddSecretSource(IConfigurationSource source)
        {
            secretSources.Add(source ?? throw new ArgumentNullException(nameof(source)));
            return this;
        }

        public IVostokConfigurationBuilder CustomizeConfigurationSource(Func<IConfigurationSource, IConfigurationSource> customization)
        {
            sourceCustomization.AddCustomization(customization ?? throw new ArgumentNullException(nameof(customization)));
            return this;
        }

        public IVostokConfigurationBuilder CustomizeSecretConfigurationSource(Func<IConfigurationSource, IConfigurationSource> customization)
        {
            secretSourceCustomization.AddCustomization(customization ?? throw new ArgumentNullException(nameof(customization)));
            return this;
        }

        public IVostokConfigurationBuilder CustomizeSettingsMerging(Action<SettingsMergeOptions> settingsCustomization)
        {
            mergeSettingsCustomization.AddCustomization(settingsCustomization ?? throw new ArgumentNullException(nameof(settingsCustomization)));
            return this;
        }

        public IVostokConfigurationBuilder CustomizeSecretSettingsMerging(Action<SettingsMergeOptions> settingsCustomization)
        {
            mergeSecretSettingsCustomization.AddCustomization(settingsCustomization ?? throw new ArgumentNullException(nameof(settingsCustomization)));
            return this;
        }

        public IVostokConfigurationBuilder CustomizeConfigurationProvider(Action<ConfigurationProviderSettings> settingsCustomization)
        {
            providerCustomization.AddCustomization(settingsCustomization ?? throw new ArgumentNullException(nameof(settingsCustomization)));
            return this;
        }

        public IVostokConfigurationBuilder CustomizeSecretConfigurationProvider(Action<ConfigurationProviderSettings> settingsCustomization)
        {
            secretProviderCustomization.AddCustomization(settingsCustomization ?? throw new ArgumentNullException(nameof(settingsCustomization)));
            return this;
        }

        public IVostokConfigurationBuilder CustomizePrintSettings(Action<PrintSettings> settingsCustomization)
        {
            printSettingsCustomization.AddCustomization(settingsCustomization ?? throw new ArgumentNullException(nameof(settingsCustomization)));
            return this;
        }

        public IVostokConfigurationBuilder CustomizeConfigurationContext(Action<IVostokConfigurationContext> configurationContextCustomization)
        {
            this.configurationContextCustomization.AddCustomization(configurationContextCustomization ?? throw new ArgumentNullException(nameof(configurationContextCustomization)));
            return this;
        }

        public (SwitchingSource source,
            SwitchingSource secretSource,
            ConfigurationProvider provider,
            ConfigurationProvider secretProvider) Build(BuildContext context)
        {
            sources.InsertRange(0, clusterConfigSources.Select(sourceProvider => sourceProvider(context.ClusterConfigClient)));

            var source = new SwitchingSource(PrepareCombinedSource(mergeSettingsCustomization, sourceCustomization, sources));
            var secretSource = new SwitchingSource(PrepareCombinedSource(mergeSecretSettingsCustomization, secretSourceCustomization, secretSources));

            var providerSettings = new ConfigurationProviderSettings()
                .WithErrorLogging(context.Log)
                .WithSettingsLogging(context.Log, printSettingsCustomization.Customize(new PrintSettings()
                {
                    InitialIndent = true
                }));

            providerCustomization.Customize(providerSettings);

            var secretProviderSettings = new ConfigurationProviderSettings()
                .WithErrorLogging(context.Log);

            secretProviderCustomization.Customize(secretProviderSettings);
            secretProviderSettings.Binder = new SecretBinder(secretProviderSettings.Binder ?? new DefaultSettingsBinder());

            var provider = new ConfigurationProvider(providerSettings);
            var secretProvider = new ConfigurationProvider(secretProviderSettings);

            var configurationContext = new ConfigurationContext(source, secretSource, provider, secretProvider, context.ClusterConfigClient);

            configurationContextCustomization.Customize(configurationContext);

            return (source, secretSource, provider, secretProvider);
        }

        private static IConfigurationSource PrepareCombinedSource(
            Customization<SettingsMergeOptions> mergeCustomization,
            Customization<IConfigurationSource> sourceCustomization,
            IReadOnlyList<IConfigurationSource> sources)
        {
            var mergeOptions = new SettingsMergeOptions();

            mergeCustomization.Customize(mergeOptions);

            if (sources.Any())
                return sourceCustomization.Customize(new CombinedSource(sources.ToArray(), mergeOptions));

            return new ConstantSource(null);
        }
    }
}