using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Models;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.Tests
{
    [TestFixture]
    internal class VostokHost_Tests_StartStop
    {
        private Application application;
        private VostokHost host;

        [SetUp]
        public void TestSetup()
        {
            application = new Application();
            host = new VostokHost(new VostokHostSettings(application, SetupEnvironment)
            {
                WarmupZooKeeper = false,
                WarmupConfiguration = false
            });
        }

        [TestCase(VostokApplicationState.EnvironmentSetup)]
        [TestCase(VostokApplicationState.EnvironmentWarmup)]
        [TestCase(VostokApplicationState.Initializing)]
        [TestCase(VostokApplicationState.Initialized)]
        [TestCase(VostokApplicationState.Running)]
        public void Start_should_wait_until_given_state_occurs(VostokApplicationState stateToAwait)
        {
            host.Start(stateToAwait);
            host.ApplicationState.Should().Match<VostokApplicationState>(state => state >= stateToAwait);
        }

        [TearDown]
        public void TearDown()
        {
            host.Stop();
            host.ApplicationState.IsTerminal().Should().BeTrue();
        }

        private class Application : IVostokApplication
        {
            public Task InitializeAsync(IVostokHostingEnvironment environment) 
                => Task.Delay(150);

            public Task RunAsync(IVostokHostingEnvironment environment)
            {
                var source = new TaskCompletionSource<bool>(false);

                environment.ShutdownToken.Register(() => source.TrySetResult(true));

                return source.Task;
            }
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
}
