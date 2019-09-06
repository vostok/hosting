using System;
using System.Threading.Tasks;
using Vostok.Hosting;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;

namespace ConsoleApp1
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var application = new Application();
            var log = new SynchronousConsoleLog();

            EnvironmentSetup environmentSetup = setup =>
            {
                setup
                    .SetupApplicationIdentity(
                        applicationIdentitySetup => applicationIdentitySetup
                            .SetProject("vostok"))
                    .SetupLog(logSetup => logSetup.AddLog(log));
            };

            var runner = new VostokHost(new VostokHostSettings(application, environmentSetup));

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