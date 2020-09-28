using System;
using Vostok.Commons.Environment;
using Vostok.Commons.Helpers;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Setup;
using Vostok.Metrics;
using Vostok.Metrics.System.Gc;
using Vostok.Metrics.System.Process;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.SystemMetrics
{
    internal class SystemMetricsBuilder : IVostokSystemMetricsBuilder
    {
        private readonly Customization<SystemMetricsSettings> settingsCustomization 
            = new Customization<SystemMetricsSettings>();

        private volatile IVostokHostingEnvironment environment;

        public IVostokSystemMetricsBuilder Customize(Action<SystemMetricsSettings> customization)
        {
            settingsCustomization.AddCustomization(customization);
            return this;
        }

        public void Customize(Action<SystemMetricsSettings, IVostokHostingEnvironment> customization)
            => Customize(settings => customization(settings, environment));

        public void Build(BuildContext context, IVostokHostingEnvironment environment)
        {
            this.environment = environment;

            var settings = settingsCustomization.Customize(new SystemMetricsSettings());

            var metricContext = context.Metrics.Instance.WithTag("SystemMetricsType", "Process");

            if (RuntimeDetector.IsDotNetCore30AndNewer)
                RegisterGcMonitor(settings, context, metricContext);

            RegisterProcessMonitor(settings, context, metricContext);
        }

        private void RegisterGcMonitor(SystemMetricsSettings settings, BuildContext context, IMetricContext metricContext)
        {
            var gcMonitor = new GarbageCollectionMonitor();

            context.HostExtensions.AsMutable().Add(gcMonitor);
            context.DisposableHostExtensions.Add(gcMonitor);

            if (settings.EnableGcEventsLogging)
                context.DisposableHostExtensions.Add(gcMonitor.LogCollections(context.Log, gc => gc.Duration >= settings.GcMinimumDurationForLogging));

            if (settings.EnableGcEventsMetrics)
                context.DisposableHostExtensions.Add(gcMonitor.ReportMetrics(metricContext));
        }

        private void RegisterProcessMonitor(SystemMetricsSettings settings, BuildContext context, IMetricContext metricContext)
        {
            var processMonitor = new CurrentProcessMonitor();

            context.HostExtensions.AsMutable().Add(processMonitor);

            if (settings.EnableProcessMetricsLogging)
                context.DisposableHostExtensions.Add(processMonitor.LogPeriodically(context.Log, settings.ProcessMetricsLoggingPeriod));

            if (settings.EnableProcessMetricsReporting)
                new CurrentProcessMetricsCollector().ReportMetrics(metricContext);
        }
    }
}
