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
        private Func<string> apiKeyProvider;
        private string stream;
        private readonly Customization<HerculesSpanSenderSettings> settingsCustomization;

        public HerculesSpanSenderBuilder()
        {
            settingsCustomization = new Customization<HerculesSpanSenderSettings>();
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
            var herculesSink = context.HerculesSink;

            if (herculesSink == null || stream == null)
                return null;

            if (apiKeyProvider != null)
                herculesSink.ConfigureStream(stream, new StreamSettings { ApiKeyProvider = apiKeyProvider });

            var settings = new HerculesSpanSenderSettings(herculesSink, stream);

            settingsCustomization.Customize(settings);

            return new HerculesSpanSender(settings);
        }
    }
}