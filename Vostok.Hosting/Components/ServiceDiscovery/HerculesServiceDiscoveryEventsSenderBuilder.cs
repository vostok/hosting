using System;
using Vostok.Commons.Helpers;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hosting.Setup;
using Vostok.ServiceDiscovery.Telemetry.Hercules;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.ServiceDiscovery
{
    internal class HerculesServiceDiscoveryEventsSenderBuilder : SwitchableComponent<IVostokHerculesServiceDiscoveryEventsSenderBuilder>,
        IVostokHerculesServiceDiscoveryEventsSenderBuilder,
        IBuilder<HerculesServiceDiscoveryEventsSender>
    {
        private readonly Customization<HerculesServiceDiscoveryEventsSenderSettings> settingsCustomization;
        private string stream;
        private Func<string> apiKeyProvider;

        public HerculesServiceDiscoveryEventsSenderBuilder()
        {
            settingsCustomization = new Customization<HerculesServiceDiscoveryEventsSenderSettings>();
        }

        public IVostokHerculesServiceDiscoveryEventsSenderBuilder SetStream(string stream)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            return this;
        }

        public IVostokHerculesServiceDiscoveryEventsSenderBuilder SetApiKeyProvider(Func<string> apiKeyProvider)
        {
            this.apiKeyProvider = apiKeyProvider ?? throw new ArgumentNullException(nameof(apiKeyProvider));
            return this;
        }

        public IVostokHerculesServiceDiscoveryEventsSenderBuilder CustomizeSettings(Action<HerculesServiceDiscoveryEventsSenderSettings> settingsCustomization)
        {
            this.settingsCustomization.AddCustomization(settingsCustomization ?? throw new ArgumentNullException(nameof(settingsCustomization)));
            return this;
        }

        public HerculesServiceDiscoveryEventsSender Build(BuildContext context)
        {
            if (!IsEnabled)
            {
                context.LogDisabled("HerculesServiceDiscoveryEventsSender");
                return null;
            }

            var herculesSink = context.HerculesSink;
            if (herculesSink == null)
            {
                context.LogDisabled("HerculesServiceDiscoveryEventsSender", "disabled HerculesSink");
                return null;
            }

            if (stream == null)
            {
                context.LogDisabled("HerculesServiceDiscoveryEventsSender", "stream not configured");
                return null;
            }

            if (apiKeyProvider != null)
                herculesSink.ConfigureStream(stream, new StreamSettings {ApiKeyProvider = apiKeyProvider});

            var settings = new HerculesServiceDiscoveryEventsSenderSettings(herculesSink, stream);
            settingsCustomization.Customize(settings);

            return new HerculesServiceDiscoveryEventsSender(settings);
        }
    }
}