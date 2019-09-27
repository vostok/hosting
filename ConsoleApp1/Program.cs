using System;
using System.Threading.Tasks;
using Vostok.Hosting;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;
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
                            .SetTracerProvider((tracerSettings, tracerLog) => new KonturTracer(tracerSettings, tracerLog)));
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
            environment.Log.Debug("Debug log.");
            environment.Log.Info("Info log.");
            environment.Log.Warn("Warn log.");
            environment.Log.Error("Error log.");

            return Task.Delay(TimeSpan.FromSeconds(6));
        }
    }
}