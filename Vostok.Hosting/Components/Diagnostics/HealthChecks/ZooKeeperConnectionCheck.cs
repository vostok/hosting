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
        private volatile bool warmedUp;

        public ZooKeeperConnectionCheck(ZooKeeperClient client)
            => this.client = client;

        public async Task<HealthCheckResult> CheckAsync(CancellationToken cancellationToken)
        {
            await WarmupIfNeededAsync().ConfigureAwait(false);

            var currentConnectionState = client.ConnectionState;

            switch (currentConnectionState)
            {
                case ConnectionState.Connected:
                case ConnectionState.ConnectedReadonly:
                    return HealthCheckResult.Healthy();
                
                default:
                    return HealthCheckResult.Degraded($"ZooKeeper client is not connected. Current connection state = '{currentConnectionState}'.");
            }
        }

        private async Task WarmupIfNeededAsync()
        {
            if (warmedUp)
                return;

            await client.ExistsAsync("/").ConfigureAwait(false);

            warmedUp = true;
        }
    }
}
