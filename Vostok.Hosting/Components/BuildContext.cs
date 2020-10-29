using System;
using System.Collections.Generic;
using Vostok.ClusterConfig.Client.Abstractions;
using Vostok.Configuration;
using Vostok.Configuration.Sources.Switching;
using Vostok.Datacenters;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Components.Diagnostics;
using Vostok.Hosting.Components.Diagnostics.InfoProviders;
using Vostok.Hosting.Components.Log;
using Vostok.Hosting.Components.Tracing;
using Vostok.Hosting.Models;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;
using Vostok.ServiceDiscovery.Abstractions;
using Vostok.Tracing;
using Vostok.Tracing.Abstractions;
using Vostok.ZooKeeper.Client.Abstractions;

// ReSharper disable SuspiciousTypeConversion.Global

namespace Vostok.Hosting.Components
{
    internal class BuildContext : IDisposable
    {
        private readonly SubstitutableLog substitutableLog;
        private readonly SubstitutableTracer substitutableTracer;

        public BuildContext()
        {
            substitutableLog = new SubstitutableLog();
            substitutableTracer = new SubstitutableTracer();
        }

        public IVostokApplicationIdentity ApplicationIdentity { get; set; }
        public IVostokApplicationLimits ApplicationLimits { get; set; }
        public Func<IVostokApplicationReplicationInfo> ApplicationReplication { get; set; }
        public IServiceLocator ServiceLocator { get; set; }
        public IServiceBeacon ServiceBeacon { get; set; }
        public IClusterConfigClient ClusterConfigClient { get; set; }
        public SwitchingSource ConfigurationSource { get; set; }
        public SwitchingSource SecretConfigurationSource { get; set; }
        public ConfigurationProvider ConfigurationProvider { get; set; }
        public ConfigurationProvider SecretConfigurationProvider { get; set; }
        public IHerculesSink HerculesSink { get; set; }
        public IVostokApplicationMetrics Metrics { get; set; }
        public ApplicationMetricsProvider MetricsInfoProvider { get; set; }
        public DiagnosticsHub DiagnosticsHub { get; set; }
        public bool ExternalZooKeeperClient { get; set; }
        public IZooKeeperClient ZooKeeperClient { get; set; }
        public IDatacenters Datacenters { get; set; }
        public IVostokHostingEnvironmentSetupContext EnvironmentSetupContext { get; set; }
        public IVostokConfigurationSetupContext ConfigurationSetupContext { get; set; }
        public IVostokHostExtensions HostExtensions { get; set; }
        public List<object> DisposableHostExtensions { get; set; }

        public Logs Logs { get; set; }
        public string LogsDirectory { get; set; }

        public ILog Log
        {
            get => substitutableLog;
            set => substitutableLog.SubstituteWith(value);
        }

        public ITracer Tracer => substitutableTracer;

        public void SubstituteTracer((ITracer tracer, TracerSettings tracerSettings) tracer)
            => substitutableTracer.SubstituteWith(tracer.tracer, tracer.tracerSettings);

        public void PrintBufferedLogs()
        {
            // Note(kungurtsev): if log hasn't been created yet, send all messages from buffer.
            Log = new SynchronousConsoleLog(new ConsoleLogSettings {ColorsEnabled = true});
        }

        public void Dispose()
        {
            try
            {
                LogDisposing("VostokHostingEnvironment");

                foreach (var hostExtension in DisposableHostExtensions ?? new List<object>())
                    TryDispose(hostExtension, $"{hostExtension.GetType().Name} extension");

                TryDispose(DiagnosticsHub, "Diagnostics");

                TryDispose(Metrics?.Root, "Metrics");

                TryDispose(ServiceBeacon, "ServiceBeacon");

                Log = Logs?.BuildCompositeLog(true) ?? new SilentLog();
                SubstituteTracer((new Tracer(new TracerSettings(new DevNullSpanSender())), new TracerSettings(new DevNullSpanSender())));
                TryDispose(HerculesSink, "HerculesSink");

                TryDispose(ServiceLocator, "ServiceLocator");

                if (!ExternalZooKeeperClient)
                    TryDispose(ZooKeeperClient, "ZooKeeperClient");

                TryDispose(Datacenters, "Datacenters");

                TryDispose(ConfigurationProvider, "ConfigurationProvider");

                TryDispose(SecretConfigurationProvider, "SecretConfigurationProvider");

                TryDispose(ClusterConfigClient, "ClusterConfigClient");

                LogDisposing("Log");
                Log = new SilentLog();
                Logs?.Dispose();
            }
            catch (Exception error)
            {
                Log.ForContext<VostokHostingEnvironment>().Error(error, "Failed to dispose of the hosting environment.");

                throw;
            }
        }

        public void LogDisabled(string name) =>
            Log.ForContext<VostokHostingEnvironment>().Info("{ComponentName} feature has been disabled.", name);

        public void LogDisabled(string name, string reason) =>
            Log.ForContext<VostokHostingEnvironment>().Info("{ComponentName} feature has been disabled due to {ComponentDisabledReason}.", name, reason);

        private void LogDisposing(string componentName) =>
            Log.ForContext<VostokHostingEnvironment>().Info("Disposing of {ComponentName}..", componentName);

        private void TryDispose(object component, string componentName)
        {
            var disposable = component as IDisposable;
            if (disposable == null)
                return;

            LogDisposing(componentName);
            disposable.Dispose();
        }
    }
}