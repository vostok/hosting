using System.Collections.Generic;
using System.Net;
using Vostok.Datacenters;

namespace Vostok.Hosting.Components.Datacenters
{
    internal class SubstitutableDatacenters : IDatacenters
    {
        private volatile IDatacenters baseDatacenters = new EmptyDatacenters();

        public string GetLocalDatacenter() =>
            baseDatacenters.GetLocalDatacenter();

        public string GetDatacenter(IPAddress address) =>
            baseDatacenters.GetDatacenter(address);

        public string GetDatacenter(string hostname) =>
            baseDatacenters.GetDatacenter(hostname);

        public string GetDatacenterWeak(string hostname) =>
            baseDatacenters.GetDatacenterWeak(hostname);

        public IReadOnlyCollection<string> GetActiveDatacenters() =>
            baseDatacenters.GetActiveDatacenters();

        public void SubstituteWith(IDatacenters newDatacenters) =>
            baseDatacenters = newDatacenters;

        public IDatacenters GetBase() =>
            baseDatacenters;
    }
}