using System.Threading;
using JetBrains.Annotations;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Diagnostics;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.Components.Diagnostics;

[PublicAPI]
public static class HealthTrackerHelper
{
    public static void LaunchPeriodicalChecks(IVostokApplicationDiagnostics diagnostics, CancellationToken cancellationToken = default)
    {
        if (diagnostics.HealthTracker is HealthTracker healthTracker)
            healthTracker.LaunchPeriodicalChecks(cancellationToken);
        else
            LogProvider.Get().Warn($"Provided {nameof(IHealthTracker)} instance is of unknown type {diagnostics.HealthTracker.GetType().Name}, unable to launch periodical health checks.");
    }
}