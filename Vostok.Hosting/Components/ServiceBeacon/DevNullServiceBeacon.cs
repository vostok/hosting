using Vostok.ServiceDiscovery.Abstractions;

namespace Vostok.Hosting.Components.ServiceBeacon
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