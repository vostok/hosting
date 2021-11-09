using System;
using System.Collections.Generic;
using Vostok.Commons.Helpers;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hosting.Setup;
using Vostok.Metrics.Hercules;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Metrics
{
    internal class HerculesMetricEventSenderBuilder : IVostokHerculesMetricEventSenderBuilder, IBuilder<HerculesMetricSender>
    {
        private readonly Customization<HerculesMetricSenderSettings> settingsCustomization;
        private readonly List<(string stream, Func<string> apiKeyProvider)> apiKeyProviderBuilders;
        private volatile bool enabled;

        public HerculesMetricEventSenderBuilder()
        {
            apiKeyProviderBuilders = new List<(string stream, Func<string> apiKeyProvider)>();
            settingsCustomization = new Customization<HerculesMetricSenderSettings>();
        }

        public bool IsEnabled => enabled;

        public IVostokHerculesMetricEventSenderBuilder Disable()
        {
            enabled = false;
            return this;
        }

        public IVostokHerculesMetricEventSenderBuilder Enable()
        {
            enabled = true;
            return this;
        }

        public IVostokHerculesMetricEventSenderBuilder SetApiKeyProvider(Func<string> apiKeyProvider, string stream = null)
        {
            apiKeyProviderBuilders.Add((stream, apiKeyProvider ?? throw new ArgumentNullException(nameof(apiKeyProvider))));
            return this;
        }

        public IVostokHerculesMetricEventSenderBuilder CustomizeSettings(Action<HerculesMetricSenderSettings> settingsCustomization)
        {
            this.settingsCustomization.AddCustomization(settingsCustomization ?? throw new ArgumentNullException(nameof(settingsCustomization)));
            return this;
        }

        public HerculesMetricSender Build(BuildContext context)
        {
            if (!enabled)
            {
                context.LogDisabled("HerculesMetricSender");
                return null;
            }

            var herculesSink = context.HerculesSink;
            if (herculesSink == null)
            {
                context.LogDisabled("HerculesMetricSender", "disabled HerculesSink");
                return null;
            }

            var settings = new HerculesMetricSenderSettings(context.HerculesSink);

            settingsCustomization.Customize(settings);

            ConfigureStreams(context, settings, herculesSink);

            return new HerculesMetricSender(settings);
        }

        private void ConfigureStreams(BuildContext context, HerculesMetricSenderSettings settings, IHerculesSink herculesSink)
        {
            var allStreams = new[]
            {
                settings.FallbackStream,
                settings.FinalStream,
                settings.CountersStream,
                settings.TimersStream,
                settings.HistogramsStream,
                settings.AnnotationsStream
            };

            foreach (var (stream, apiKeyProvider) in apiKeyProviderBuilders)
            {
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