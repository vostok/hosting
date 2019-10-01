using Vostok.ServiceDiscovery.Abstractions;

namespace Vostok.Hosting.Components.ServiceDiscovery
{
    internal class DevNullServiceLocator : IServiceLocator
    {
        public IServiceTopology Locate(string environment, string application) => null;
    }
}