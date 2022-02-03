using System;
using System.Collections.Generic;
using System.Linq;
using Vostok.ClusterConfig.Client.Abstractions;
using Vostok.Commons.Helpers;
using Vostok.Commons.Helpers.Disposable;
using Vostok.Configuration;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.Merging;
using Vostok.Configuration.Binders;
using Vostok.Configuration.Logging;
using Vostok.Configuration.Printing;
using Vostok.Configuration.Sources;
using Vostok.Configuration.Sources.Combined;
using Vostok.Configuration.Sources.Constant;
using Vostok.Configuration.Sources.Switching;
using Vostok.Context;
using Vostok.Hosting.Setup;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Configuration
{
    internal class ConfigurationBuilder : IVostokConfigurationBuilder,
        IBuilder<(SwitchingSource source, SwitchingSource secretSource, ConfigurationProvider provider, ConfigurationProvider secretProvider)>
    {
        private readonly List<Source> sources;

        private readonly Customization<SettingsMergeOptions> mergeSettingsCustomization;
        private readonly Customization<SettingsMergeOptions> mergeSecretSettingsCustomization;
        private readonly Customization<SettingsMergeOptions> mergeMergedSettingsCustomization;
        
        private readonly Customization<IConfigurationSource> sourceCustomization;
        private readonly Customization<IConfigurationSource> secretSourceCustomization;
        private readonly Customization<IConfigurationSource> mergedSourceCustomization;
        
        private readonly Customization<ConfigurationProviderSettings> providerCustomization;
        private readonly Customization<ConfigurationProviderSettings> secretProviderCustomization;
        
        private readonly Customization<PrintSettings> printSettingsCustomization;
        private readonly Customization<IVostokConfigurationContext> configurationContextCustomization;
        

        public ConfigurationBuilder()
        {
            sources = new List<Source>();
            mergeSettingsCustomization = new Customization<SettingsMergeOptions>();
            mergeSecretSettingsCustomization = new Customization<SettingsMergeOptions>();
            mergeMergedSettingsCustomization = new Customization<SettingsMergeOptions>();
            providerCustomization = new Customization<ConfigurationProviderSettings>();
            secretProviderCustomization = new Customization<ConfigurationProviderSettings>();
            printSettingsCustomization = new Customization<PrintSettings>();
            configurationContextCustomization = new Customization<IVostokConfigurationContext>();
            sourceCustomization = new Customization<IConfigurationSource>();
            secretSourceCustomization = new Customization<IConfigurationSource>();
            mergedSourceCustomization = new Customization<IConfigurationSource>();
        }

        public IVostokConfigurationSourcesBuilder AddSource(IConfigurationSource source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            sources.Add(new Source(false, _ => source));
            return this;
        }

        public IVostokConfigurationSourcesBuilder AddSource(Func<IClusterConfigClient, IConfigurationSource> sourceProvider)
        {
            if (sourceProvider == null)
                throw new ArgumentNullException(nameof(sourceProvider));
            sources.Add(new Source(false, sourceProvider));
            return this;
        }

        public IVostokConfigurationSourcesBuilder AddSecretSource(IConfigurationSource source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            sources.Add(new Source(true, _ => source));
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
        
        public IVostokConfigurationBuilder CustomizeMergedConfigurationSource(Func<IConfigurationSource, IConfigurationSource> customization)
        {
            mergedSourceCustomization.AddCustomization(customization ?? throw new ArgumentNullException(nameof(customization)));
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
        
        public IVostokConfigurationBuilder CustomizeMergedSettingsMerging(Action<SettingsMergeOptions> settingsCustomization)
        {
            mergeMergedSettingsCustomization.AddCustomization(settingsCustomization ?? throw new ArgumentNullException(nameof(settingsCustomization)));
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

        public TSettings GetIntermediateConfiguration<TSettings>(params string[] scope)
        {
            using (var provider = BuildProvider(Context))
                return provider.Get<TSettings>(BuildSource(Context).ScopeTo(scope));
        }

        public TSettings GetIntermediateSecretConfiguration<TSettings>(params string[] scope)
        {
            using (var secretProvider = BuildSecretProvider(Context))
                return secretProvider.Get<TSettings>(BuildSecretSource(Context).ScopeTo(scope));
        }

        public TSettings GetIntermediateMergedConfiguration<TSettings>(params string[] scope)
        {
            using (var provider = BuildProvider(Context))
                return provider.Get<TSettings>(BuildMergedSource(Context).ScopeTo(scope));
        }

        public (SwitchingSource source,
            SwitchingSource secretSource,
            ConfigurationProvider provider,
            ConfigurationProvider secretProvider) Build(BuildContext context)
        {
            var source = BuildSource(context);
            var secretSource = BuildSecretSource(context);
            var mergedSource = BuildMergedSource(context);

            var provider = BuildProvider(context);
            var secretProvider = BuildSecretProvider(context);

            var configurationContext = new ConfigurationContext(source, secretSource, mergedSource, provider, secretProvider, context.ClusterConfigClient);

            configurationContextCustomization.Customize(configurationContext);

            return (source, secretSource, provider, secretProvider);
        }

        public static IDisposable UseContext(BuildContext context)
        {
            var wrapper = new BuildContextWrapper {Value = context};
            var contextToken = FlowingContext.Globals.Use(wrapper);

            return new ActionDisposable(() =>
            {
                // note (kungurtsev, 16.11.2021): use wrapper to avoid context leak to started threads
                wrapper.Value = null;
                contextToken.Dispose();
            });
        }

        private static BuildContext Context
            => FlowingContext.Globals.Get<BuildContextWrapper>()?.Value ?? throw new InvalidOperationException();

        private ConfigurationProvider BuildProvider(BuildContext context)
        {
            var providerSettings = new ConfigurationProviderSettings()
                .WithErrorLogging(context.Log)
                .WithSettingsLogging(context.Log,
                    printSettingsCustomization.Customize(new PrintSettings
                    {
                        InitialIndent = true
                    }));

            providerCustomization.Customize(providerSettings);

            return new ConfigurationProvider(providerSettings);
        }

        private ConfigurationProvider BuildSecretProvider(BuildContext context)
        {
            var secretProviderSettings = new ConfigurationProviderSettings()
                .WithErrorLogging(context.Log)
                .WithUpdatesLogging(context.Log);

            secretProviderCustomization.Customize(secretProviderSettings);
            secretProviderSettings.Binder = new SecretBinder(secretProviderSettings.Binder ?? new DefaultSettingsBinder());

            return new ConfigurationProvider(secretProviderSettings);
        }

        private SwitchingSource BuildSource(BuildContext context)
        {
            var clusterConfigClient = GetClusterConfigClient(context);
            var sourcesList = sources.Where(s => !s.Secret).Select(s => s.Factory(clusterConfigClient)).ToArray();
            return new SwitchingSource(PrepareCombinedSource(mergeSettingsCustomization, sourceCustomization, sourcesList));
        }
        
        private SwitchingSource BuildSecretSource(BuildContext context)
        {
            var clusterConfigClient = GetClusterConfigClient(context);
            var sourcesList = sources.Where(s => s.Secret).Select(s => s.Factory(clusterConfigClient)).ToArray();
            return new SwitchingSource(PrepareCombinedSource(mergeSecretSettingsCustomization, secretSourceCustomization, sourcesList));
        }
        
        private SwitchingSource BuildMergedSource(BuildContext context)
        {
            var clusterConfigClient = GetClusterConfigClient(context);
            var sourcesList = sources.Select(s => s.Factory(clusterConfigClient)).ToArray();
            return new SwitchingSource(PrepareCombinedSource(mergeMergedSettingsCustomization, mergedSourceCustomization, sourcesList));
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
        
        private static IClusterConfigClient GetClusterConfigClient(BuildContext context)
            => context.ClusterConfigClient ?? throw new ArgumentNullException(nameof(BuildContext.ClusterConfigClient), "ClusterConfig client hasn't been built. This is most likely a bug.");

        private class BuildContextWrapper
        {
            public BuildContext Value { get; set; }
        }
        
        private class Source
        {
            public bool Secret { get; }
            public Func<IClusterConfigClient, IConfigurationSource> Factory { get; }

            public Source(bool secret, Func<IClusterConfigClient, IConfigurationSource> factory)
            {
                Secret = secret;
                Factory = factory;
            }
        }
    }
}