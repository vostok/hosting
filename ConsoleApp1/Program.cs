using System;
using System.Text;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Transport;
using Vostok.Hosting;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;
using Vostok.Metrics;
using Vostok.Metrics.Models;
using Vostok.Telemetry.Kontur;
using Vostok.Tracing.Abstractions;
using Vostok.Tracing.Kontur;

namespace ConsoleApp1
{
    internal static class Program
    {
        private static void Main()
        {
            var application = new Application();
            var log = new SynchronousConsoleLog(new ConsoleLogSettings {ColorsEnabled = true});

            EnvironmentSetup<IEnvironmentBuilder> innerSetup = setup =>
            {
                setup
                    .SetupApplicationIdentity(
                        applicationIdentitySetup => applicationIdentitySetup
                            .SetProject("Infrastructure")
                            //.SetSubproject("vostok")
                            .SetEnvironmentFromClusterConfig("app/environment")
                            .SetApplication("vostok-hosting-test")
                            .SetInstance("1")
                    )
                    .SetupHerculesSink(
                        herculesSinkSetup => herculesSinkSetup
                            .SetClusterConfigApiKeyProvider("app/apiKey")
                            .SetClusterConfigClusterProvider("topology/hercules/gate.prod")
                            .SuppressVerboseLogging()
                    )
                    .SetupLog(
                        logSetup => logSetup
                            .AddLog(log)
                            .SetupHerculesLog(
                                herculesLogSetup => herculesLogSetup
                                    .SetStream("logs_vostoklibs_cloud")
                                    .AddAdditionalLogTransformation(
                                        l => l
                                            .WithMinimumLevel(LogLevel.Info)))
                    )
                    .SetupTracer(
                        tracerSetup => tracerSetup
                            .SetTracerProvider((tracerSettings, tracerLog) => new KonturTracer(tracerSettings, tracerLog))
                            .SetupHerculesSpanSender(
                                spanSenderSetup => spanSenderSetup
                                    .SetStreamFromClusterConfig("vostok/tracing/StreamName"))
                        .AddSpanSender(new LogSpanSender(log))
                    )
                    .SetupMetrics(
                        metricsSetup => metricsSetup
                            .AddMetricEventSenderSender(new LogMetricEventSender(log))
                    )
                    .SetupClusterClientSetup(
                        clusterClientSetup => { clusterClientSetup.SetupDistributedKonturTracing(); }
                    )
                    .SetupZooKeeperClient(
                        zooKeeperClientSetup => zooKeeperClientSetup
                            .SetClusterConfigClusterProvider("topology/zookeeper-global")
                    )
                    ;
            };

            EnvironmentSetup<IEnvironmentBuilder> outerSetup = setup =>
            {
                innerSetup(setup);
                setup
                    .SetupLog(
                        logSetup => logSetup
                            .SetupHerculesLog(
                                herculesLogSetup => herculesLogSetup
                                    .AddAdditionalLogTransformation(
                                        l => l.WithProperty("outer", "value"))));
            };

            var runner = new VostokHost(new VostokHostSettings(application, outerSetup));

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                runner.ShutdownTokenSource.Cancel();
            };

            var result = runner.RunAsync().GetAwaiter().GetResult();

            log.Info($"RunResult: {result.Status} {result.Error}");
        }
    }

    internal class Application : IVostokApplication
    {
        public Task InitializeAsync(IVostokHostingEnvironment environment)
        {
            return Task.Delay(TimeSpan.FromSeconds(3));
        }

        public Task RunAsync(IVostokHostingEnvironment environment)
        {
            var log = environment.Log.ForContext<Application>();

            log.Debug("Debug log.");
            log.Info("Info log.");
            log.Warn("Warn log.");
            log.Error("Error log.");

            environment.Metrics.Instance.Send(new MetricDataPoint(42, "point"));

            var client = new ClusterClient(
                log,
                setup =>
                {
                    setup.SetupUniversalTransport();
                    setup.ClusterProvider = new FixedClusterProvider("https://google.com");
                    environment.ClusterClientSetup(setup);
                });

            var responce = client.Send(Request.Get(""));

            return Task.Delay(TimeSpan.FromSeconds(15));
        }
    }

    internal class LogSpanSender : ISpanSender
    {
        private readonly ILog log;

        public LogSpanSender(ILog log)
        {
            this.log = log.ForContext<LogSpanSender>();
        }

        public void Send(ISpan span)
        {
            var sb = new StringBuilder();

            sb.Append($"TraceId: {span.TraceId}. ");

            if (span.Annotations.ContainsKey(WellKnownAnnotations.Http.Request.TargetService))
                sb.Append($"TargetService: {span.Annotations[WellKnownAnnotations.Http.Request.TargetService]}. ");

            //log.Info(sb.ToString());
        }
    }

    internal class LogMetricEventSender : IMetricEventSender
    {
        private readonly ILog log;

        public LogMetricEventSender(ILog log)
        {
            this.log = log.ForContext<LogMetricEventSender>();
        }

        public void Send(MetricEvent @event)
        {
            //log.Info($"{@event.Value} {@event.Tags}");
        }
    }
}