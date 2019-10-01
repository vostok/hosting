using Vostok.ServiceDiscovery.Abstractions;

namespace Vostok.Hosting.Components.ServiceDiscovery
{
    internal class DevNullServiceBeacon : IServiceBeacon
    {
        public void Start()
        {
        }

        public void Stop()
        {
        }

        public IReplicaInfo ReplicaInfo { get; }
    }
}