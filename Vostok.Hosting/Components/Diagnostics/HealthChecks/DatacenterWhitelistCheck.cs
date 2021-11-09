using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Datacenters;
using Vostok.Hosting.Abstractions.Diagnostics;

namespace Vostok.Hosting.Components.Diagnostics.HealthChecks
{
    [PublicAPI]
    public class DatacenterWhitelistCheck : IHealthCheck
    {
        private readonly IDatacenters datacenters;

        public DatacenterWhitelistCheck(IDatacenters datacenters)
            => this.datacenters = datacenters;

        public Task<HealthCheckResult> CheckAsync(CancellationToken cancellationToken)
        {
            var localDatacenter = datacenters.GetLocalDatacenter();
            if (localDatacenter == null)
                return Task.FromResult(HealthCheckResult.Healthy());

            var activeDatacenters = datacenters.GetActiveDatacenters();
            if (activeDatacenters.Contains(localDatacenter))
                return Task.FromResult(HealthCheckResult.Healthy());

            return Task.FromResult(HealthCheckResult.Failing($"Local datacenter '{localDatacenter}' is not among active datacenters ({string.Join(", ", activeDatacenters)})."));
        }
    }
}