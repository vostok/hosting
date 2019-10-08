using System;
using System.Collections.Generic;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hosting.Components.String;
using Vostok.Hosting.Setup;
using Vostok.Metrics;
using Vostok.Metrics.Hercules;

namespace Vostok.Hosting.Components.Metrics
{
    internal class HerculesMetricEventSenderBuilder : IVostokHerculesMetricEventSenderBuilder, IBuilder<IMetricEventSender>
    {
        private List<(string stream, StringProviderBuilder builder)> apiKeyProviderBuilders;

        private StringProviderBuilder fallbackStreamProviderBuilder;
        private StringProviderBuilder finalStreamProviderBuilder;
        private StringProviderBuilder countersStreamProviderBuilder;
        private StringProviderBuilder timersStreamProviderBuilder;
        private StringProviderBuilder histogramsStreamProviderBuilder;

        public HerculesMetricEventSenderBuilder()
        {
            apiKeyProviderBuilders = new List<(string stream, StringProviderBuilder builder)>();
        }

        #region StreamsSettings

        public IVostokHerculesMetricEventSenderBuilder SetFallbackStream(string stream)
        {
            fallbackStreamProviderBuilder = StringProviderBuilder.FromValue(stream);
            return this;
        }

        public IVostokHerculesMetricEventSenderBuilder SetFallbackStreamFromClusterConfig(string path)
        {
            fallbackStreamProviderBuilder = StringProviderBuilder.FromClusterConfig(path);
            return this;
        }

        public IVostokHerculesMetricEventSenderBuilder SetFinalStream(string stream)
        {
            finalStreamProviderBuilder = StringProviderBuilder.FromValue(stream);
            return this;
        }

        public IVostokHerculesMetricEventSenderBuilder SetFinalStreamFromClusterConfig(string path)
        {
            finalStreamProviderBuilder = StringProviderBuilder.FromClusterConfig(path);
            return this;
        }

        public IVostokHerculesMetricEventSenderBuilder SetCountersStream(string stream)
        {
            countersStreamProviderBuilder = StringProviderBuilder.FromValue(stream);
            return this;
        }

        public IVostokHerculesMetricEventSenderBuilder SetCountersStreamFromClusterConfig(string path)
        {
            countersStreamProviderBuilder = StringProviderBuilder.FromClusterConfig(path);
            return this;
        }

        public IVostokHerculesMetricEventSenderBuilder SetTimersStream(string stream)
        {
            timersStreamProviderBuilder = StringProviderBuilder.FromValue(stream);
            return this;
        }

        public IVostokHerculesMetricEventSenderBuilder SetTimersStreamFromClusterConfig(string path)
        {
            timersStreamProviderBuilder = StringProviderBuilder.FromClusterConfig(path);
            return this;
        }

        public IVostokHerculesMetricEventSenderBuilder SetHistogramsStream(string stream)
        {
            histogramsStreamProviderBuilder = StringProviderBuilder.FromValue(stream);
            return this;
        }

        public IVostokHerculesMetricEventSenderBuilder SetHistogramsStreamFromClusterConfig(string path)
        {
            histogramsStreamProviderBuilder = StringProviderBuilder.FromClusterConfig(path);
            return this;
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

        #endregion

        public IMetricEventSender Build(BuildContext context)
        {
            var herculesSink = context.HerculesSink;

            if (herculesSink == null)
                return null;

            var settings = BuildSettings(context);

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

        private HerculesMetricSenderSettings BuildSettings(BuildContext context)
        {
            var settings = new HerculesMetricSenderSettings(context.HerculesSink);

            var fallbackStream = fallbackStreamProviderBuilder?.Build(context)?.Invoke();
            if (fallbackStream != null)
                settings.FallbackStream = fallbackStream;

            var finalStream = finalStreamProviderBuilder?.Build(context)?.Invoke();
            if (finalStream != null)
                settings.FinalStream = finalStream;

            var countersStream = countersStreamProviderBuilder?.Build(context)?.Invoke();
            if (countersStream != null)
                settings.CountersStream = countersStream;

            var timersStream = timersStreamProviderBuilder?.Build(context)?.Invoke();
            if (timersStream != null)
                settings.TimersStream = timersStream;

            var histogramStream = histogramsStreamProviderBuilder?.Build(context)?.Invoke();
            if (histogramStream != null)
                settings.HistogramsStream = histogramStream;

            return settings;
        }
    }
}