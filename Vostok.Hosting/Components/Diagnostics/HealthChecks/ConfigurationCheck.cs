using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Configuration;
using Vostok.Hosting.Abstractions.Diagnostics;

namespace Vostok.Hosting.Components.Diagnostics.HealthChecks;

[PublicAPI]
public class ConfigurationCheck : IHealthCheck
{
    private readonly ConfigurationProvider configurationProvider;

    public ConfigurationCheck(ConfigurationProvider configurationProvider) =>
        this.configurationProvider = configurationProvider;

    public Task<HealthCheckResult> CheckAsync(CancellationToken cancellationToken)
    {
        var reason = configurationProvider.GetHealthStatus();

        var result = new HealthCheckResult(reason == null ? HealthStatus.Healthy : HealthStatus.Degraded, reason);

        return Task.FromResult(result);
    }
}