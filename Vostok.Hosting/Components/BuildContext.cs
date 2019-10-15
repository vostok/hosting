using System;
using System.Threading;
using Vostok.ClusterConfig.Client.Abstractions;
using Vostok.Configuration.Abstractions;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Components.Log;
using Vostok.Hosting.Components.Tracing;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;
using Vostok.Logging.File;
using Vostok.ServiceDiscovery.Abstractions;
using Vostok.Tracing;
using Vostok.Tracing.Abstractions;
using Vostok.ZooKeeper.Client.Abstractions;

namespace Vostok.Hosting.Components
{
    internal class BuildContext : IDisposable
    {
        public CancellationToken ShutdownToken { get; set; }
        public IVostokApplicationIdentity ApplicationIdentity { get; set; }
        public IServiceLocator ServiceLocator { get; set; }
        public IServiceBeacon ServiceBeacon { get; set; }
        public IClusterConfigClient ClusterConfigClient { get; set; }
        public IConfigurationSource ConfigurationSource { get; set; }
        public IConfigurationProvider ConfigurationProvider { get; set; }
        public IHerculesSink HerculesSink { get; set; }
        public IVostokApplicationMetrics Metrics { get; set; }
        public IZooKeeperClient ZooKeeperClient { get; set; }

        public IVostokHostingEnvironmentSetupContext SetupContext { get; set; }

        public Logs Logs { get; set; }
        
        public ILog Log
        {
            get => substitutableLog;
            set => substitutableLog.SubstituteWith(value);
        }

        public ITracer Tracer
        {
            get => substitutableTracer;
        }

        public void SubstituteTracer((ITracer tracer, TracerSettings tracerSettings) tracer)
        {
            substitutableTracer.SubstituteWith(tracer.tracer, tracer.tracerSettings);
        }

        public void PrintBufferedLogs()
        {
            // Note(kungurtsev): if log hasn't created yet, send all messages from buffer.
            Log = new SynchronousConsoleLog(new ConsoleLogSettings { ColorsEnabled = true });
        }

        private readonly SubstitutableLog substitutableLog;

        private readonly SubstitutableTracer substitutableTracer;
        
        public BuildContext()
        {
            substitutableLog = new SubstitutableLog();
            substitutableTracer = new SubstitutableTracer();
        }

        public void Dispose()
        {
            try
            {
                Log = Logs?.BuildCompositeLog(true) ?? new SilentLog();
                SubstituteTracer((new DevNullTracer(), new TracerSettings(new DevNullSpanSender())));

                (Metrics?.Root as IDisposable)?.Dispose();

                (HerculesSink as IDisposable)?.Dispose();

                (ServiceBeacon as IDisposable)?.Dispose();
                (ServiceLocator as IDisposable)?.Dispose();
                (ZooKeeperClient as IDisposable)?.Dispose();

                (ClusterConfigClient as IDisposable)?.Dispose();
                
                Log = new SilentLog();

                Logs?.Dispose();
            }
            catch (Exception error)
            {
                Log.Error(error, "Failed to dispose vostok hosting environment.");

                throw;
            }
        }
    }
}