using System;
using System.Threading.Tasks;
using Vostok.Hosting;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;

namespace ConsoleApp1
{
    internal static class Program
    {
        private static void Main()
        {
            var application = new Application();
            var log = new SynchronousConsoleLog();

            EnvironmentSetup<IEnvironmentBuilder> innerSetup = setup =>
            {
                setup
                    .SetupApplicationIdentity(
                        applicationIdentitySetup => applicationIdentitySetup
                            .SetProject("Infrastructure")
                            //.SetSubproject("vostok")
                            .SetEnvironment("dev")
                            .SetApplication("vostok-hosting-test")
                            .SetInstance("1"))
                    .SetupHerculesSink(
                        herculesSinkSetup => herculesSinkSetup
                            .SetApiKeyProvider(() => "vostoklibs_telemetry_write_key_cloud_df237b91706e4626a299653c8a30e140")
                            .SetClusterConfigClusterProvider("topology/hercules/gate.prod"))
                    .SetupLog(
                        logSetup => logSetup
                            .AddLog(log)
                            .SetupHerculesLog(
                                herculesLogSetup => herculesLogSetup
                                    .SetStream("logs_vostoklibs_cloud")
                                    .WithAdditionalLogTransformation(
                                        l => l
                                            .WithMinimumLevel(LogLevel.Info))));
            };

            EnvironmentSetup<IEnvironmentBuilder> outerSetup = setup =>
            {
                innerSetup(setup);
                setup
                    .SetupLog(
                        logSetup => logSetup
                            .SetupHerculesLog(
                                herculesLogSetup => herculesLogSetup
                                    .WithAdditionalLogTransformation(
                                        l => l.WithProperty("outer", "value"))));
            };

            var runner = new VostokHost(new VostokHostSettings(application, outerSetup));

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                runner.ShutdownTokenSource.Cancel();
            };

            var result = runner.RunAsync().GetAwaiter().GetResult();

            log.Info($"Result: {result.Status} {result.Error}");
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
            return Task.Delay(TimeSpan.FromSeconds(6));
        }
    }
}