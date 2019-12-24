using System;
using System.Threading;
using Vostok.ClusterConfig.Client.Abstractions;
using Vostok.Configuration.Abstractions;
using Vostok.Datacenters;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hosting.Abstractions;
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
        public IServiceLocator ServiceLocator { get; set; }
        public IServiceBeacon ServiceBeacon { get; set; }
        public IClusterConfigClient ClusterConfigClient { get; set; }
        public IConfigurationSource ConfigurationSource { get; set; }
        public IConfigurationSource SecretConfigurationSource { get; set; }
        public IConfigurationProvider ConfigurationProvider { get; set; }
        public IConfigurationProvider SecretConfigurationProvider { get; set; }
        public IHerculesSink HerculesSink { get; set; }
        public IVostokApplicationMetrics Metrics { get; set; }
        public IZooKeeperClient ZooKeeperClient { get; set; }
        public IDatacenters Datacenters { get; set; }

        public IVostokHostingEnvironmentSetupContext SetupContext { get; set; }

        public Logs Logs { get; set; }

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

                LogDisposing("Metrics");
                (Metrics?.Root as IDisposable)?.Dispose();

                LogDisposing("ServiceBeacon");
                (ServiceBeacon as IDisposable)?.Dispose();

                LogDisposing("ServiceLocator");
                (ServiceLocator as IDisposable)?.Dispose();

                LogDisposing("ZooKeeperClient");
                (ZooKeeperClient as IDisposable)?.Dispose();

                Log = Logs?.BuildCompositeLog(true) ?? new SilentLog();
                SubstituteTracer((new DevNullTracer(), new TracerSettings(new DevNullSpanSender())));

                LogDisposing("HerculesSink");
                (HerculesSink as IDisposable)?.Dispose();

                LogDisposing("Datacenters");
                (Datacenters as IDisposable)?.Dispose();

                LogDisposing("ConfigurationProvider");
                (ConfigurationProvider as IDisposable)?.Dispose();

                LogDisposing("SecretConfigurationProvider");
                (SecretConfigurationProvider as IDisposable)?.Dispose();

                LogDisposing("ClusterConfigClient");
                (ClusterConfigClient as IDisposable)?.Dispose();

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
    }
}