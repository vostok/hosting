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
        var reason = configurationProvider.GetHealthCheckResult();

        var result = new HealthCheckResult(reason.Error == null ? HealthStatus.Healthy : HealthStatus.Degraded, reason.Error);

        return Task.FromResult(result);
    }
}