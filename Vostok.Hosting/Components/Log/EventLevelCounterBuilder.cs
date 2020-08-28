using System;
using Vostok.Commons.Helpers;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.Components.Log
{
    internal class EventLevelCounterBuilder : IVostokEventLevelCounterBuilder
    {
        private readonly Customization<EventLevelCounterSettings> settingsCustomization
            = new Customization<EventLevelCounterSettings>();

        public bool EventLevelCounterEnabled => settingsCustomization.Customize(new EventLevelCounterSettings()).EnableEventLevelCounter;

        public IVostokEventLevelCounterBuilder Customize(Action<EventLevelCounterSettings> customization)
        {
            settingsCustomization.AddCustomization(customization);
            return this;
        }

        public EventLevelCounter Build(BuildContext context)
        {
            // For future settings.
            var settings = settingsCustomization.Customize(new EventLevelCounterSettings());

            return new EventLevelCounter();
        }
    }
}