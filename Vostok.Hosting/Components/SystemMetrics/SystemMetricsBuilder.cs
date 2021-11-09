using System;
using Vostok.Commons.Environment;
using Vostok.Commons.Helpers;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Setup;
using Vostok.Metrics;
using Vostok.Metrics.System.Dns;
using Vostok.Metrics.System.Gc;
using Vostok.Metrics.System.Host;
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

            var processMetricsContext = context.Metrics.Instance.WithTag("SystemMetricsType", "Process");
            var hostMetricsContext = context.Metrics.Instance.WithTag("SystemMetricsType", "Host");

            if (RuntimeDetector.IsDotNetCore30AndNewer)
                RegisterGcMonitor(settings, context, processMetricsContext);

            if (RuntimeDetector.IsDotNet50AndNewer)
                RegisterDnsMonitor(settings, context, processMetricsContext);

            RegisterProcessMonitor(settings, context, processMetricsContext);

            RegisterHostMonitor(settings, context, hostMetricsContext);
        }

        private void RegisterGcMonitor(SystemMetricsSettings settings, BuildContext context, IMetricContext metricContext)
        {
            var gcMonitor = new GarbageCollectionMonitor();

            context.HostExtensions.AsMutable().Add(gcMonitor);

            if (settings.EnableGcEventsLogging)
                context.RegisterDisposable(gcMonitor.LogCollections(context.Log, gc => gc.Duration >= settings.GcMinimumDurationForLogging));

            if (settings.EnableGcEventsMetrics)
                context.RegisterDisposable(gcMonitor.ReportMetrics(metricContext));

            context.RegisterDisposable(gcMonitor);
        }

        private void RegisterProcessMonitor(SystemMetricsSettings settings, BuildContext context, IMetricContext metricContext)
        {
            var processMonitor = new CurrentProcessMonitor();

            context.HostExtensions.AsMutable().Add(processMonitor);

            if (settings.EnableProcessMetricsLogging)
                context.RegisterDisposable(processMonitor.LogPeriodically(context.Log, settings.ProcessMetricsLoggingPeriod));

            if (settings.EnableProcessMetricsReporting)
            {
                var collectorSettings = new CurrentProcessMetricsSettings
                {
                    CpuCoresLimitProvider = () => context.ApplicationLimits.CpuUnits,
                    MemoryBytesLimitProvider = () => context.ApplicationLimits.MemoryBytes
                };

                var collector = new CurrentProcessMetricsCollector(collectorSettings);
                context.RegisterDisposable(collector.ReportMetrics(metricContext, settings.ProcessMetricsReportingPeriod));
                context.RegisterDisposable(collector);
            }

            context.RegisterDisposable(processMonitor);
        }

        private void RegisterHostMonitor(SystemMetricsSettings settings, BuildContext context, IMetricContext metricContext)
        {
            var hostMetricsSettings = settings.HostMetricsSettings;
            var hostMonitor = new HostMonitor(hostMetricsSettings);

            context.HostExtensions.AsMutable().Add(hostMonitor);

            if (settings.EnableHostMetricsLogging)
                context.RegisterDisposable(hostMonitor.LogPeriodically(context.Log, settings.HostMetricsLoggingPeriod));

            if (settings.EnableHostMetricsReporting)
            {
                var collector = new HostMetricsCollector(hostMetricsSettings);
                context.RegisterDisposable(collector.ReportMetrics(metricContext, settings.HostMetricsReportingPeriod));
                context.RegisterDisposable(collector);
            }

            context.RegisterDisposable(hostMonitor);
        }

        private void RegisterDnsMonitor(SystemMetricsSettings settings, BuildContext context, IMetricContext metricContext)
        {
            var dnsMonitor = new DnsMonitor();

            context.HostExtensions.AsMutable().Add(dnsMonitor);

            if (settings.EnableDnsEventsMetrics)
                context.RegisterDisposable(dnsMonitor.ReportMetrics(metricContext));

            context.RegisterDisposable(dnsMonitor);
        }
    }
}