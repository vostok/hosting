using System.Threading;
using System.Threading.Tasks;
using Vostok.Hosting.Abstractions.Diagnostics;
using Vostok.ZooKeeper.Client;
using Vostok.ZooKeeper.Client.Abstractions.Model;

namespace Vostok.Hosting.Components.Diagnostics.HealthChecks
{
    internal class ZooKeeperConnectionCheck : IHealthCheck
    {
        private readonly ZooKeeperClient client;

        public ZooKeeperConnectionCheck(ZooKeeperClient client)
            => this.client = client;

        public Task<HealthCheckResult> CheckAsync(CancellationToken cancellationToken)
        {
            var currentConnectionState = client.ConnectionState;

            switch (currentConnectionState)
            {
                case ConnectionState.Connected:
                case ConnectionState.ConnectedReadonly:
                    return Task.FromResult(HealthCheckResult.Healthy());
                
                default:
                    return Task.FromResult(HealthCheckResult.Degraded($"ZooKeeper client is not connected. Current connection state = '{currentConnectionState}'."));
            }
        }
    }
}
