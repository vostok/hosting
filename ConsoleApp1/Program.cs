﻿using System;
using System.Text;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Transport;
using Vostok.Configuration.Sources.Object;
using Vostok.Hosting;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;
using Vostok.Logging.File.Configuration;
using Vostok.Metrics;
using Vostok.Metrics.Models;
using Vostok.ServiceDiscovery.Kontur;
using Vostok.Tracing.Abstractions;
using Vostok.Tracing.Kontur;

// ReSharper disable AssignNullToNotNullAttribute

namespace ConsoleApp1
{
    internal static class Program
    {
        private static void Main()
        {
            var application = new MyApplication();
            var log = new SynchronousConsoleLog(new ConsoleLogSettings {ColorsEnabled = true});

            VostokHostingEnvironmentSetup innerSetup = setup =>
            {
                setup
                    .SetupApplicationIdentity(
                        (applicationIdentitySetup, setupContext) => applicationIdentitySetup
                            .SetProject("Infrastructure")
                            //.SetSubproject("vostok")
                            .SetEnvironment(setupContext.ClusterConfigClient.Get("app/environment")?.Value)
                            .SetApplication("vostok-hosting-test")
                            .SetInstance("1")
                    )
                    .SetupHerculesSink(
                        (herculesSinkSetup, setupContext) => herculesSinkSetup
                            .SetApiKeyProvider(() => setupContext.ClusterConfigClient.Get("app/apiKey")?.Value)
                            .SetClusterConfigClusterProvider("topology/hercules/gate.prod")
                            //.SuppressVerboseLogging()
                            .CustomizeSettings(
                                customization =>
                                {
                                    //customization.SendPeriod = TimeSpan.FromSeconds(1);
                                })
                    )
                    .SetupLog(
                        (logSetup, setupContext) =>
                        {
                            setupContext.Log.Info("Here we can log something, about log configuration!");
                            //throw new Exception("Test error.");

                            logSetup
                                //.AddLog(log)
                                .SetupFileLog(
                                    fileLogSetup => fileLogSetup
                                        .CustomizeSettings(
                                            customization =>
                                            {
                                                customization.RollingStrategy = new RollingStrategyOptions()
                                                {
                                                    Type = RollingStrategyType.ByTime
                                                };
                                            }))
                                .SetupHerculesLog(
                                    herculesLogSetup => herculesLogSetup
                                        .SetStream("logs_vostoklibs_cloud")
                                        .CustomizeLog(
                                            l => l
                                                .WithMinimumLevel(LogLevel.Info)))
                                .SetupConsoleLog(
                                    consoleLogSetup => consoleLogSetup
                                        .Enable());
                        })
                    .SetupTracer(
                        (tracerSetup, setupContext) => tracerSetup
                            .SetTracerProvider((tracerSettings, tracerLog) => new KonturTracer(tracerSettings, tracerLog))
                            .SetupHerculesSpanSender(
                                spanSenderSetup => spanSenderSetup
                                    .SetStream(setupContext.ClusterConfigClient.Get("vostok/tracing/StreamName")?.Value))
                            .AddSpanSender(new LogSpanSender(log))
                    )
                    .SetupMetrics(
                        metricsSetup => metricsSetup
                            .AddMetricEventSender(new LogMetricEventSender(log))
                            .SetupHerculesMetricEventSender(
                                herculesMetricEventSenderSetup => herculesMetricEventSenderSetup
                                    .CustomizeSettings(
                                        customization =>
                                        {
                                            //customization.FinalStream = "123";
                                        }))
                    )
                    .SetupClusterClientSetup(
                        clusterClientSetup => clusterClientSetup
                            .SetupTracing(
                                tracingSetup => tracingSetup
                                    .CustomizeSettings(
                                        customization => { customization.AdditionalRequestTransformation = (request, context) => request.WithHeader("bla", context.TraceId); }))
                    )
                    .SetupZooKeeperClient(
                        zooKeeperClientSetup => zooKeeperClientSetup
                            .SetClusterConfigClusterProvider("topology/zookeeper-global")
                            .CustomizeSettings(
                                customization =>
                                {
                                    //customization.LoggingLevel = LogLevel.Debug;
                                })
                    )
                    .SetupServiceBeacon(
                        serviceBeaconSetup => serviceBeaconSetup
                            .SetupReplicaInfo(
                                replicaInfoSetup => replicaInfoSetup
                                    .SetPort(42)
                            )
                            .CustomizeSettings(
                                customization => { customization.ZooKeeperNodesPathEscaper = KonturZooKeeperPathEscaper.Instance; })
                    )
                    .SetupServiceLocator(
                        serviceLocatorSetup => serviceLocatorSetup
                            .CustomizeSettings(
                                customization => { customization.ZooKeeperNodesPathEscaper = KonturZooKeeperPathEscaper.Instance; })
                    )
                    .SetupClusterConfigClient(
                        clusterConfigClientSetup => clusterConfigClientSetup
                            .CustomizeSettings(
                                settings =>
                                {
                                    //settings.Zone = "123";
                                })
                    )
                    .SetupConfiguration(
                        configurationSetup => configurationSetup
                            .CustomizeConfigurationContext(
                                setupContext => { setupContext.ConfigurationProvider.SetupSourceFor<MySettings>(new ObjectSource(new MySettings {Value = "my_value"})); })
                    )
                    ;
            };

            VostokHostingEnvironmentSetup outerSetup = setup =>
            {
                innerSetup(setup);
                setup
                    .SetupLog(
                        logSetup => logSetup
                            .SetupHerculesLog(
                                herculesLogSetup => herculesLogSetup
                                    .CustomizeLog(
                                        l => l.WithProperty("outer", "value"))));
            };

            // Way 1: use host.
            var runner = new VostokHost(new VostokHostSettings(application, outerSetup));

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                runner.ShutdownTokenSource.Cancel();
            };

            var result = runner.RunAsync().GetAwaiter().GetResult();

            log.Info($"RunResult: {result.Status} {result.Error}");

            // Way 2: build environment without host.
            // var environment = VostokHostingEnvironmentFactory.Build(outerSetup);
        }
    }

    internal class MyApplication : IVostokApplication
    {
        public Task InitializeAsync(IVostokHostingEnvironment environment)
        {
            return Task.Delay(TimeSpan.FromSeconds(3));
        }

        public Task RunAsync(IVostokHostingEnvironment environment)
        {
            var log = environment.Log.ForContext<MyApplication>();

            log.Debug("Debug log.");
            log.Info("Info log.");
            log.Warn("Warn log.");
            log.Error("Error log.");

            var settings = environment.ConfigurationProvider.Get<MySettings>();
            log.Info("Settings value: {Value}.", settings.Value);

            environment.Metrics.Instance.Send(new MetricDataPoint(42, "point"));

            var client = new ClusterClient(
                log,
                setup =>
                {
                    setup.SetupUniversalTransport();
                    setup.ClusterProvider = new FixedClusterProvider("https://google.com");
                    environment.ClusterClientSetup(setup);
                });

            var spansearchapi = environment.ServiceLocator.Locate("default", "vostok.spansearchapi");

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

            foreach (var annotation in span.Annotations)
            {
                sb.Append($"{annotation.Key}: {annotation.Value} ");
            }

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

    internal class MySettings
    {
        public string Value;
    }
}