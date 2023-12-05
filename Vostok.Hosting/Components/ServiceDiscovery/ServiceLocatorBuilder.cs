using System;
using Vostok.Commons.Helpers;
using Vostok.Hosting.Components.ZooKeeper;
using Vostok.Hosting.Setup;
using Vostok.ServiceDiscovery;
using Vostok.ServiceDiscovery.Abstractions;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.ServiceDiscovery
{
    internal class ServiceLocatorBuilder : IVostokServiceLocatorBuilder, IBuilder<IServiceLocator>
    {
        public readonly Customization<IServiceLocator> StaticProviderCustomization;
        private readonly Customization<ServiceLocatorSettings> settingsCustomization;

        public ServiceLocatorBuilder()
        {
            StaticProviderCustomization = new Customization<IServiceLocator>();
            settingsCustomization = new Customization<ServiceLocatorSettings>();
        }

        public IServiceLocator Build(BuildContext context)
        {
            var zooKeeperClient = context.ZooKeeperClient;
            if (zooKeeperClient == null || zooKeeperClient is DevNullZooKeeperClient)
            {
                context.LogDisabled("ServiceLocator", "disabled ZooKeeperClient");
                return new DevNullServiceLocator();
            }

            var settings = new ServiceLocatorSettings();

            settingsCustomization.Customize(settings);

            return new ServiceLocator(zooKeeperClient, settings);
        }

        public IVostokServiceLocatorBuilder CustomizeSettings(Action<ServiceLocatorSettings> settingsCustomization)
        {
            this.settingsCustomization.AddCustomization(settingsCustomization ?? throw new ArgumentNullException(nameof(settingsCustomization)));
            return this;
        }

        public IVostokServiceLocatorBuilder ConfigureStaticProvider(Action<IServiceLocator> configure)
        {
            StaticProviderCustomization.AddCustomization(configure ?? throw new ArgumentNullException(nameof(configure)));

            return this;
        }
    }
}