using System;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hosting.Components.String;
using Vostok.Hosting.Helpers;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Hercules;
using Vostok.Logging.Hercules.Configuration;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Log
{
    internal class HerculesLogBuilder : LogBuilderBase, IVostokHerculesLogBuilder
    {
        private StringProviderBuilder apiKeyProviderBuilder;
        private StringProviderBuilder streamProviderBuilder;
        private readonly SettingsCustomization<HerculesLogSettings> settingsCustomization;

        public HerculesLogBuilder()
        {
            settingsCustomization = new SettingsCustomization<HerculesLogSettings>();
        }

        public IVostokHerculesLogBuilder SetStream(string stream)
        {
            streamProviderBuilder = StringProviderBuilder.FromValue(stream);
            return this;
        }

        public IVostokHerculesLogBuilder SetStreamFromClusterConfig(string path)
        {
            apiKeyProviderBuilder = StringProviderBuilder.FromClusterConfig(path);
            return this;
        }

        public IVostokHerculesLogBuilder SetApiKeyProvider(Func<string> apiKeyProvider)
        {
            apiKeyProviderBuilder = StringProviderBuilder.FromValueProvider(apiKeyProvider);
            return this;
        }

        public IVostokHerculesLogBuilder SetClusterConfigApiKeyProvider(string path)
        {
            apiKeyProviderBuilder = StringProviderBuilder.FromClusterConfig(path);
            return this;
        }

        public IVostokHerculesLogBuilder CustomizeLog(Func<ILog, ILog> additionalTransformation)
        {
            LogCustomizations.Add(additionalTransformation);
            return this;
        }

        public IVostokHerculesLogBuilder CustomizeSettings(Action<HerculesLogSettings> settingsCustomization)
        {
            this.settingsCustomization.AddCustomization(settingsCustomization);
            return this;
        }

        protected override ILog BuildInner(BuildContext context)
        {
            var herculesSink = context.HerculesSink;
            var stream = streamProviderBuilder?.Build(context)?.Invoke();

            // CR(iloktionov): Let's log something about every component that's not working as intended due to missing configuration.
            if (herculesSink == null || stream == null)
                return null;

            var apiKeyProvider = apiKeyProviderBuilder?.Build(context);
            if (apiKeyProvider != null)
                herculesSink.ConfigureStream(stream, new StreamSettings {ApiKeyProvider = apiKeyProvider});

            var settings = new HerculesLogSettings(herculesSink, stream);

            settingsCustomization.Customize(settings);

            return new HerculesLog(settings);
        }
    }
}