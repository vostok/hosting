using System;
using JetBrains.Annotations;
using Vostok.Hosting.Components.SystemMetrics;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokSystemMetricsBuilder
    {
        [NotNull]
        IVostokSystemMetricsBuilder Customize([NotNull] Action<SystemMetricsSettings> customization);
    }
}