using System;
using Vostok.Hosting.Helpers;
using Vostok.Hosting.Setup;
using Vostok.ServiceDiscovery;
using Vostok.ServiceDiscovery.Abstractions;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.ServiceDiscovery
{
    internal class ServiceLocatorBuilder : IVostokServiceLocatorBuilder, IBuilder<IServiceLocator>
    {
        private readonly Customization<ServiceLocatorSettings> settingsCustomization;

        public ServiceLocatorBuilder()
        {
            settingsCustomization = new Customization<ServiceLocatorSettings>();
        }

        public IServiceLocator Build(BuildContext context)
        {
            var zooKeeperClient = context.ZooKeeperClient;
            if (zooKeeperClient == null)
            {
                context.Log.LogDisabled("ServiceLocator", "disabled ZooKeeperClient");
                return new DevNullServiceLocator();
            }

            var settings = new ServiceLocatorSettings();

            settingsCustomization.Customize(settings);

            return new ServiceLocator(zooKeeperClient, settings);
        }

        public IVostokServiceLocatorBuilder CustomizeSettings(Action<ServiceLocatorSettings> settingsCustomization)
        {
            this.settingsCustomization.AddCustomization(settingsCustomization);
            return this;
        }
    }
}