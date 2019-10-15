using Vostok.ServiceDiscovery.Abstractions;

namespace Vostok.Hosting.Components.ServiceDiscovery
{
    internal class DevNullServiceBeacon : IServiceBeacon
    {
        public IReplicaInfo ReplicaInfo { get; }

        public void Start()
        {
        }

        public void Stop()
        {
        }
    }
}