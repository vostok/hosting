using System;
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
        private IVostokApplication application;
        private VostokHost host;

        [TestCase(VostokApplicationState.EnvironmentSetup)]
        [TestCase(VostokApplicationState.EnvironmentWarmup)]
        [TestCase(VostokApplicationState.Initializing)]
        [TestCase(VostokApplicationState.Initialized)]
        [TestCase(VostokApplicationState.Running)]
        public void Start_should_wait_until_given_state_occurs(VostokApplicationState stateToAwait)
        {
            application = new Application();
            host = new VostokHost(new VostokHostSettings(application, SetupEnvironment)
            {
                WarmupZooKeeper = false,
                WarmupConfiguration = false
            });

            host.Start(stateToAwait);
            host.ApplicationState.Should().Match<VostokApplicationState>(state => state >= stateToAwait);

            host.Stop();
            host.ApplicationState.IsTerminal().Should().BeTrue();
        }

        [Test]
        public void Start_should_throw_on_initialize_fail()
        {
            application = new BadApplication(true);
            host = new VostokHost(new VostokHostSettings(application, SetupEnvironment)
            {
                WarmupZooKeeper = false,
                WarmupConfiguration = false
            });
            
            Action checkStart = () => host.Start(VostokApplicationState.Initialized);
            checkStart.Should().Throw<Exception>().WithMessage("initialize");

            Action checkStop = () => host.Stop();
            checkStop.Should().Throw<Exception>().WithMessage("initialize");
        }

        [Test]
        public void Start_should_not_throw_on_run_fail()
        {
            application = new BadApplication(false);
            host = new VostokHost(new VostokHostSettings(application, SetupEnvironment)
            {
                WarmupZooKeeper = false,
                WarmupConfiguration = false
            });

            Action checkStart = () => host.Start(VostokApplicationState.Initialized);
            checkStart.Should().NotThrow();

            Action checkStop = () => host.Stop();
            checkStop.Should().Throw<Exception>().WithMessage("run");
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

        private class BadApplication : IVostokApplication
        {
            private readonly bool failInitialize;

            public BadApplication(bool failInitialize) =>
                this.failInitialize = failInitialize;

            public Task InitializeAsync(IVostokHostingEnvironment environment)
                => failInitialize ? throw new Exception("initialize") : Task.CompletedTask;

            public Task RunAsync(IVostokHostingEnvironment environment)
                => throw new Exception("run");
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
