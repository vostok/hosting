using System;
using Vostok.Commons.Helpers;
using Vostok.Hosting.Setup;
using Vostok.ServiceDiscovery;
using Vostok.ServiceDiscovery.Abstractions;

namespace Vostok.Hosting.Components.ServiceDiscovery
{
    internal class ServiceDiscoveryManagerBuilder : IVostokServiceDiscoveryManagerBuilder, IBuilder<IServiceDiscoveryManager>
    {
        private readonly Customization<ServiceDiscoveryManagerSettings> settingsCustomization;

        public ServiceDiscoveryManagerBuilder()
        {
            settingsCustomization = new Customization<ServiceDiscoveryManagerSettings>();
        }

        public IServiceDiscoveryManager Build(BuildContext context)
        {
            var zooKeeperClient = context.ZooKeeperClient;
            if (zooKeeperClient == null)
            {
                context.LogDisabled("ServiceDiscoveryManager", "disabled ZooKeeperClient");
                return null;
            }
            
            var settings = new ServiceDiscoveryManagerSettings();

            settingsCustomization.Customize(settings);
            
            return new ServiceDiscoveryManager(zooKeeperClient, settings);
        }

        public IVostokServiceDiscoveryManagerBuilder CustomizeSettings(Action<ServiceDiscoveryManagerSettings> settingsCustomization)
        {    
            this.settingsCustomization.AddCustomization(settingsCustomization ?? throw new ArgumentNullException(nameof(settingsCustomization)));
            return this;
        }
    }
}