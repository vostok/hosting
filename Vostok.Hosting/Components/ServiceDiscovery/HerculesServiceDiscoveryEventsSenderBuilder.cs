using System;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hosting.Setup;
using Vostok.ServiceDiscovery.Telemetry.Hercules;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.ServiceDiscovery
{
    internal class HerculesServiceDiscoveryEventsSenderBuilder : IVostokHerculesServiceDiscoveryEventsSenderBuilder, IBuilder<HerculesServiceDiscoveryEventsSender>
    {
        private string stream;
        private Func<string> apiKeyProvider;
        private volatile bool enabled;

        public bool IsEnabled => enabled;

        public IVostokHerculesServiceDiscoveryEventsSenderBuilder Enable()
        {
            enabled = true;
            return this;
        }

        public IVostokHerculesServiceDiscoveryEventsSenderBuilder Disable()
        {
            enabled = false;
            return this;
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

        public HerculesServiceDiscoveryEventsSender Build(BuildContext context)
        {
            if (!enabled)
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

            return new HerculesServiceDiscoveryEventsSender(settings);
        }
    }
}