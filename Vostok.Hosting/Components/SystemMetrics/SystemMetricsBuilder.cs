using System;
using Vostok.Commons.Helpers;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Setup;
using Vostok.Metrics;
using Vostok.Metrics.System.Gc;
using Vostok.Metrics.System.Process;

namespace Vostok.Hosting.Components.SystemMetrics
{
    internal class SystemMetricsBuilder : IVostokSystemMetricsBuilder
    {
        private readonly Customization<SystemMetricsSettings> settingsCustomization 
            = new Customization<SystemMetricsSettings>();

        public IVostokSystemMetricsBuilder Customize(Action<SystemMetricsSettings> customization)
        {
            settingsCustomization.AddCustomization(customization);
            return this;
        }

        public void Build(BuildContext context)
        {
            var settings = settingsCustomization.Customize(new SystemMetricsSettings());

            var gcMonitor = new GarbageCollectionMonitor();
            var processMonitor = new CurrentProcessMonitor();

            context.HostExtensions.AsMutable().Add(gcMonitor);
            context.HostExtensions.AsMutable().Add(processMonitor);
            context.DisposableHostExtensions.Add(gcMonitor);

            var metricContext = context.Metrics.Instance.WithTag("SystemMetricsType", "Process");

            if (settings.EnableGcEventsLogging)
                context.DisposableHostExtensions.Add(gcMonitor.LogCollections(context.Log, gc => gc.Duration >= settings.GcMinimumDurationForLogging));

            if (settings.EnableGcEventsMetrics)
                context.DisposableHostExtensions.Add(gcMonitor.ReportMetrics(metricContext));

            if (settings.EnableProcessMetricsLogging)
                context.DisposableHostExtensions.Add(processMonitor.LogPeriodically(context.Log, settings.ProcessMetricsLoggingPeriod));

            if (settings.EnableProcessMetricsReporting)
                new CurrentProcessMetricsCollector().ReportMetrics(metricContext);
        }
    }
}
