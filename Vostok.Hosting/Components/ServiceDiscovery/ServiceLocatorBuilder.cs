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
        private readonly SettingsCustomization<ServiceLocatorSettings> settingsCustomization;

        public ServiceLocatorBuilder()
        {
            settingsCustomization = new SettingsCustomization<ServiceLocatorSettings>();
        }

        public IServiceLocator Build(BuildContext context)
        {
            var zooKeeperClient = context.ZooKeeperClient;

            if (zooKeeperClient == null)
                return new DevNullServiceLocator();

            return new ServiceLocator(zooKeeperClient, new ServiceLocatorSettings());
        }

        public IVostokServiceLocatorBuilder CustomizeSettings(Action<ServiceLocatorSettings> settingsCustomization)
        {
            this.settingsCustomization.AddCustomization(settingsCustomization);
            return this;
        }
    }
}