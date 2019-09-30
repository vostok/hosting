using System;
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
using Vostok.Telemetry.Kontur;
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
                            .SetInstance("1"))
                    .SetupHerculesSink(
                        herculesSinkSetup => herculesSinkSetup
                            .SetClusterConfigApiKeyProvider("app/apiKey")
                            .SetClusterConfigClusterProvider("topology/hercules/gate.prod"))
                    .SetupLog(
                        logSetup => logSetup
                            .AddLog(log)
                            .SetupHerculesLog(
                                herculesLogSetup => herculesLogSetup
                                    .SetStream("logs_vostoklibs_cloud")
                                    .AddAdditionalLogTransformation(
                                        l => l
                                            .WithMinimumLevel(LogLevel.Info))))
                    .SetupTracer(
                        tracerSetup => tracerSetup
                            .SetTracerProvider((tracerSettings, tracerLog) => new KonturTracer(tracerSettings, tracerLog))
                            .SetupHerculesSpanSender(
                                spanSenderSetup => spanSenderSetup
                                    .SetStreamFromClusterConfig("vostok/tracing/StreamName")))
                    .SetupClusterClient(
                        clusterClientSetup =>
                        {
                            clusterClientSetup.SetupDistributedKonturTracing();
                        });
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
}