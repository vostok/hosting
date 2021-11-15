using System;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Hosting;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Models;
using Vostok.Hosting.Setup;

namespace ConsoleApplication1
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            WeakReference appWeak = null, hostWeak = null;

            var app = new Application();
            var host = new VostokHost(new VostokHostSettings(app, SetupEnvironment) {ConfigureStaticProviders = false});

            appWeak = new WeakReference(app);
            hostWeak = new WeakReference(host);

            host.Run();

            app = null;
            host = null;
            
            GC.Collect(2, GCCollectionMode.Forced, true, true);
            
            // should be False False
            Console.WriteLine(appWeak.IsAlive);
            Console.WriteLine(hostWeak.IsAlive);
        }
        
        private static void SetupEnvironment(IVostokHostingEnvironmentBuilder builder)
        {
            builder.SetupApplicationIdentity(
                id =>
                {
                    id.SetProject("infra");
                    id.SetApplication("vostok-test");
                    id.SetEnvironment("dev");
                    id.SetInstance("the only one");
                });

            builder.SetupLog(log => log.SetupConsoleLog());
        }
    }

    internal class Application : IVostokApplication
    {
        public Task InitializeAsync(IVostokHostingEnvironment environment) =>
            Task.CompletedTask;

        public Task RunAsync(IVostokHostingEnvironment environment) =>
            Task.CompletedTask;
    }
}