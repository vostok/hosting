using Vostok.ServiceDiscovery.Abstractions;

namespace Vostok.Hosting.Components.ServiceDiscovery
{
    internal class DevNullServiceBeacon : IServiceBeacon
    {
        public IReplicaInfo ReplicaInfo => new EmptyReplicaInfo();

        public void Start()
        {
        }

        public void Stop()
        {
        }
    }
}