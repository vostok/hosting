using System;
using Vostok.Commons.Helpers;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hosting.Setup;
using Vostok.Tracing.Abstractions;
using Vostok.Tracing.Hercules;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Tracing
{
    internal class HerculesSpanSenderBuilder : SwitchableComponent<IVostokHerculesSpanSenderBuilder>,
        IVostokHerculesSpanSenderBuilder,
        IBuilder<ISpanSender>
    {
        private readonly Customization<HerculesSpanSenderSettings> settingsCustomization;
        private volatile Func<string> apiKeyProvider;
        private volatile string stream;

        public HerculesSpanSenderBuilder()
            => settingsCustomization = new Customization<HerculesSpanSenderSettings>();

        public IVostokHerculesSpanSenderBuilder SetStream(string stream)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            return this;
        }

        public IVostokHerculesSpanSenderBuilder SetApiKeyProvider(Func<string> apiKeyProvider)
        {
            this.apiKeyProvider = apiKeyProvider ?? throw new ArgumentNullException(nameof(apiKeyProvider));
            return this;
        }

        public IVostokHerculesSpanSenderBuilder CustomizeSettings(Action<HerculesSpanSenderSettings> settingsCustomization)
        {
            this.settingsCustomization.AddCustomization(settingsCustomization ?? throw new ArgumentNullException(nameof(settingsCustomization)));
            return this;
        }

        public ISpanSender Build(BuildContext context)
        {
            if (!IsEnabled)
            {
                context.LogDisabled("HerculesSpanSender");
                return null;
            }

            var herculesSink = context.HerculesSink;
            if (herculesSink == null)
            {
                context.LogDisabled("HerculesSpanSender", "disabled HerculesSink");
                return null;
            }

            if (stream == null)
            {
                context.LogDisabled("HerculesSpanSender", "unconfigured stream");
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