using System;
using Vostok.Commons.Helpers;
using Vostok.Hosting.Setup;
using Vostok.ServiceDiscovery.Telemetry;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.ServiceDiscovery
{
    internal class ServiceDiscoveryEventsContextBuilder : IVostokServiceDiscoveryEventsContextBuilder, IBuilder<IServiceDiscoveryEventsContext>
    {
        private readonly Customization<ServiceDiscoveryEventsContextConfig> settingsCustomization;
        private readonly HerculesServiceDiscoveryEventsSenderBuilder herculesServiceDiscoveryEventsSenderBuilder;

        public ServiceDiscoveryEventsContextBuilder()
        {
            settingsCustomization = new Customization<ServiceDiscoveryEventsContextConfig>();
            herculesServiceDiscoveryEventsSenderBuilder = new HerculesServiceDiscoveryEventsSenderBuilder();
        }

        public IVostokServiceDiscoveryEventsContextBuilder SetupHerculesServiceDiscoveryEventsSender(Action<IVostokHerculesServiceDiscoveryEventsSenderBuilder> herculesEventsSenderSetup)
        {
            herculesEventsSenderSetup = herculesEventsSenderSetup ?? throw new ArgumentNullException(nameof(herculesEventsSenderSetup));
            
            herculesServiceDiscoveryEventsSenderBuilder.Enable();
            herculesEventsSenderSetup(herculesServiceDiscoveryEventsSenderBuilder);
            return this;
        }

        public IVostokServiceDiscoveryEventsContextBuilder CustomizeSettings(Action<ServiceDiscoveryEventsContextConfig> settingsCustomization)
        {
            this.settingsCustomization.AddCustomization(settingsCustomization ?? throw new ArgumentNullException(nameof(settingsCustomization)));
            return this;
        }

        public IServiceDiscoveryEventsContext Build(BuildContext context)
        {
            var herculesEventsSender = herculesServiceDiscoveryEventsSenderBuilder.Build(context);
            if (herculesEventsSender == null)
            {
                context.LogDisabled("ServiceDiscoveryEventsContext", "disabled HerculesServiceDiscoveryEventsSender");
                return new DevNullServiceDiscoveryEventsContext();
            }

            var setting = new ServiceDiscoveryEventsContextConfig(herculesEventsSender);
            settingsCustomization.Customize(setting);

            return new ServiceDiscoveryEventsContext(setting);
        }
    }
}