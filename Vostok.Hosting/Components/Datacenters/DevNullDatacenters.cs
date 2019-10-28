using System.Collections.Generic;
using System.Net;
using Vostok.Datacenters;

namespace Vostok.Hosting.Components.Datacenters
{
    internal class DevNullDatacenters : IDatacenters
    {
        private const string DefaultDatacenter = "default";
        private static readonly List<string> DefaultDatacenters = new List<string> {DefaultDatacenter};

        public string GetLocalDatacenter() =>
            DefaultDatacenter;

        public string GetDatacenter(IPAddress address) =>
            DefaultDatacenter;

        public string GetDatacenter(string hostname) =>
            DefaultDatacenter;

        public IReadOnlyCollection<string> GetActiveDatacenters() =>
            DefaultDatacenters;
    }
}