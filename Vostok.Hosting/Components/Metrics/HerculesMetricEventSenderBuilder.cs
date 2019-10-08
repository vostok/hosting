using System;
using System.Collections.Generic;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hosting.Components.String;
using Vostok.Hosting.Helpers;
using Vostok.Hosting.Setup;
using Vostok.Metrics;
using Vostok.Metrics.Hercules;
// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Metrics
{
    internal class HerculesMetricEventSenderBuilder : IVostokHerculesMetricEventSenderBuilder, IBuilder<IMetricEventSender>
    {
        private List<(string stream, StringProviderBuilder builder)> apiKeyProviderBuilders;
        private readonly SettingsCustomization<HerculesMetricSenderSettings> settingsCustomization;

        public HerculesMetricEventSenderBuilder()
        {
            apiKeyProviderBuilders = new List<(string stream, StringProviderBuilder builder)>();
            settingsCustomization = new SettingsCustomization<HerculesMetricSenderSettings>();
        }

        public IVostokHerculesMetricEventSenderBuilder SetApiKeyProvider(Func<string> apiKeyProvider, string stream = null)
        {
            apiKeyProviderBuilders.Add((stream, StringProviderBuilder.FromValueProvider(apiKeyProvider)));
            return this;
        }

        public IVostokHerculesMetricEventSenderBuilder SetClusterConfigApiKeyProvider(string path, string stream = null)
        {
            apiKeyProviderBuilders.Add((stream, StringProviderBuilder.FromClusterConfig(path)));
            return this;
        }

        public IVostokHerculesMetricEventSenderBuilder CustomizeSettings(Action<HerculesMetricSenderSettings> settingsCustomization)
        {
            this.settingsCustomization.AddCustomization(settingsCustomization);
            return this;
        }

        public IMetricEventSender Build(BuildContext context)
        {
            var herculesSink = context.HerculesSink;

            if (herculesSink == null)
                return null;

            var settings = new HerculesMetricSenderSettings(context.HerculesSink);

            settingsCustomization.Customize(settings);

            ConfigureStreams(context, settings, herculesSink);

            return new HerculesMetricSender(settings);
        }

        private void ConfigureStreams(BuildContext context, HerculesMetricSenderSettings settings, IHerculesSink herculesSink)
        {
            var allStreams = new[] {settings.FallbackStream, settings.FinalStream, settings.CountersStream, settings.TimersStream, settings.HistogramsStream};

            foreach (var (stream, builder) in apiKeyProviderBuilders)
            {
                var apiKeyProvider = builder?.Build(context);
                if (apiKeyProvider == null)
                    continue;

                if (stream != null)
                {
                    herculesSink.ConfigureStream(stream, new StreamSettings {ApiKeyProvider = apiKeyProvider});
                }
                else
                {
                    foreach (var s in allStreams)
                    {
                        herculesSink.ConfigureStream(s, new StreamSettings {ApiKeyProvider = apiKeyProvider});
                    }
                }
            }
        }
    }
}