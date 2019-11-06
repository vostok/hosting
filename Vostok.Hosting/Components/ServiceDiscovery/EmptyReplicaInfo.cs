using System.Collections.Generic;
using Vostok.ServiceDiscovery.Abstractions;

namespace Vostok.Hosting.Components.ServiceDiscovery
{
    internal class EmptyReplicaInfo : IReplicaInfo
    {
        public string Environment => "empty";
        public string Application => "empty";
        public string Replica => "empty";
        public IReadOnlyDictionary<string, string> Properties => new Dictionary<string, string>();
    }
}