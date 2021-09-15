using JetBrains.Annotations;
using Vostok.ServiceDiscovery.Abstractions;

namespace Vostok.Hosting.Components.ServiceDiscovery
{
    [PublicAPI]
    public class DevNullServiceLocator : IServiceLocator
    {
        public IServiceTopology Locate(string environment, string application) => null;
    }
}