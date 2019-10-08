using System;
// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Helpers
{
    internal class SettingsCustomization<T>
    {
        private Action<T> customization;

        public void AddCustomization(Action<T> customization)
        {
            var previousCustomization = customization;

            this.customization = settings =>
            {
                previousCustomization?.Invoke(settings);
                customization(settings);
            };
        }

        public void Customize(T settings)
        {
            customization?.Invoke(settings);
        }
    }
}