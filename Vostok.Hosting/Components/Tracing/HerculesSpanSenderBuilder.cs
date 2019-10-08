using System;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hosting.Components.String;
using Vostok.Hosting.Helpers;
using Vostok.Hosting.Setup;
using Vostok.Tracing.Abstractions;
using Vostok.Tracing.Hercules;
using Vostok.ZooKeeper.Client;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Tracing
{
    internal class HerculesSpanSenderBuilder : IVostokHerculesSpanSenderBuilder, IBuilder<ISpanSender>
    {
        private StringProviderBuilder apiKeyProviderBuilder;
        private StringProviderBuilder streamProviderBuilder;
        private readonly SettingsCustomization<HerculesSpanSenderSettings> settingsCustomization;

        public HerculesSpanSenderBuilder()
        {
            settingsCustomization = new SettingsCustomization<HerculesSpanSenderSettings>();
        }

        public IVostokHerculesSpanSenderBuilder SetStream(string stream)
        {
            streamProviderBuilder = StringProviderBuilder.FromValue(stream);
            return this;
        }

        public IVostokHerculesSpanSenderBuilder SetStreamFromClusterConfig(string path)
        {
            streamProviderBuilder = StringProviderBuilder.FromClusterConfig(path);
            return this;
        }

        public IVostokHerculesSpanSenderBuilder SetApiKeyProvider(Func<string> apiKeyProvider)
        {
            apiKeyProviderBuilder = StringProviderBuilder.FromValueProvider(apiKeyProvider);
            return this;
        }

        public IVostokHerculesSpanSenderBuilder SetClusterConfigApiKeyProvider(string path)
        {
            apiKeyProviderBuilder = StringProviderBuilder.FromClusterConfig(path);
            return this;
        }

        public IVostokHerculesSpanSenderBuilder CustomizeSettings(Action<HerculesSpanSenderSettings> settingsCustomization)
        {
            this.settingsCustomization.AddCustomization(settingsCustomization);
            return this;
        }

        public ISpanSender Build(BuildContext context)
        {
            var herculesSink = context.HerculesSink;
            var stream = streamProviderBuilder?.Build(context)?.Invoke();

            if (herculesSink == null || stream == null)
                return null;

            var apiKeyProvider = apiKeyProviderBuilder?.Build(context);
            if (apiKeyProvider != null)
                herculesSink.ConfigureStream(stream, new StreamSettings { ApiKeyProvider = apiKeyProvider });

            var settings = new HerculesSpanSenderSettings(herculesSink, stream);

            settingsCustomization.Customize(settings);

            return new HerculesSpanSender(settings);
        }
    }
}