using System;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hosting.Helpers;
using Vostok.Hosting.Setup;
using Vostok.Tracing.Abstractions;
using Vostok.Tracing.Hercules;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Tracing
{
    internal class HerculesSpanSenderBuilder : IVostokHerculesSpanSenderBuilder, IBuilder<ISpanSender>
    {
        private readonly Customization<HerculesSpanSenderSettings> settingsCustomization;
        private volatile Func<string> apiKeyProvider;
        private volatile string stream;
        private volatile bool enabled;

        public HerculesSpanSenderBuilder()
            => settingsCustomization = new Customization<HerculesSpanSenderSettings>();

        public IVostokHerculesSpanSenderBuilder Enable()
        {
            enabled = true;
            return this;
        }

        public IVostokHerculesSpanSenderBuilder Disable()
        {
            enabled = false;
            return this;
        }

        public IVostokHerculesSpanSenderBuilder SetStream(string stream)
        {
            this.stream = stream;
            return this;
        }

        public IVostokHerculesSpanSenderBuilder SetApiKeyProvider(Func<string> apiKeyProvider)
        {
            this.apiKeyProvider = apiKeyProvider;
            return this;
        }

        public IVostokHerculesSpanSenderBuilder CustomizeSettings(Action<HerculesSpanSenderSettings> settingsCustomization)
        {
            this.settingsCustomization.AddCustomization(settingsCustomization);
            return this;
        }

        public ISpanSender Build(BuildContext context)
        {
            if (!enabled)
            {
                context.Log.LogDisabled("HerculesSpanSender");
                return null;
            }

            var herculesSink = context.HerculesSink;
            if (herculesSink == null)
            {
                context.Log.LogDisabled("HerculesSpanSender", "disabled HerculesSink");
                return null;
            }

            if (stream == null)
            {
                context.Log.LogDisabled("HerculesSpanSender", "unconfigured stream");
                return null;
            }

            if (apiKeyProvider != null)
                herculesSink.ConfigureStream(stream, new StreamSettings {ApiKeyProvider = apiKeyProvider});

            var settings = new HerculesSpanSenderSettings(herculesSink, stream);

            settingsCustomization.Customize(settings);

            return new HerculesSpanSender(settings);
        }
    }
}