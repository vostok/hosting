using System;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Requirements;
using Vostok.Hosting.Models;
using Vostok.Hosting.Setup;
using Vostok.ZooKeeper.Client.Abstractions;
using Vostok.ZooKeeper.Client.Abstractions.Model;
using Vostok.ZooKeeper.Client.Abstractions.Model.Request;
using Vostok.ZooKeeper.Client.Abstractions.Model.Result;

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
            host = new VostokHost(new TestHostSettings(application, SetupEnvironment));

            host.Start(stateToAwait);
            host.ApplicationState.Should().Match<VostokApplicationState>(state => state >= stateToAwait);

            host.Stop();
            host.ApplicationState.IsTerminal().Should().BeTrue();
        }

        [Test]
        public void Start_should_throw_on_initialize_fail()
        {
            application = new BadApplication(true);
            host = new VostokHost(new TestHostSettings(application, SetupEnvironment));

            var checkStart = () => host.Start(VostokApplicationState.Initialized);
            checkStart.Should().Throw<Exception>().WithMessage("initialize");

            Action checkStop = () => host.Stop();
            checkStop.Should().Throw<Exception>().WithMessage("initialize");
        }

        [Test]
        public void Start_should_not_throw_on_run_fail()
        {
            application = new BadApplication(false);
            host = new VostokHost(new TestHostSettings(application, SetupEnvironment));

            var checkStart = () => host.Start(VostokApplicationState.Initialized);
            checkStart.Should().NotThrow();

            Action checkStop = () => host.Stop();
            checkStop.Should().Throw<Exception>().WithMessage("run");
        }

        [Test]
        public void Start_should_check_that_beacon_has_started()
        {
            var zkClient = Substitute.For<IZooKeeperClient>();
            zkClient.CreateAsync(Arg.Any<CreateRequest>()).Returns(Task.FromResult(CreateResult.Unsuccessful(ZooKeeperStatus.AuthFailed, "", null)));

            application = new PortRequiresApplication();
            host = new VostokHost(
                new TestHostSettings(
                    application,
                    s =>
                    {
                        SetupEnvironment(s);
                        s.SetupZooKeeperClient(zkSetup => zkSetup.UseInstance(zkClient));
                        s.SetupServiceBeacon(
                            beaconSetup => beaconSetup.SetupReplicaInfo(
                                replicaInfoSetup => { replicaInfoSetup.SetApplication("auth-test"); }));
                    })
                {
                    BeaconRegistrationWaitEnabled = true,
                    BeaconRegistrationTimeout = 2.Seconds()
                });

            var checkStart = () => host.Start();
            checkStart.Should().Throw<Exception>().Where(e => e.Message.Contains("beacon hasn't registered"));

            host.ApplicationState.Should().Be(VostokApplicationState.CrashedDuringInitialization);
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

        [RequiresPort]
        private class PortRequiresApplication : IVostokApplication
        {
            public Task InitializeAsync(IVostokHostingEnvironment environment) => Task.CompletedTask;

            public Task RunAsync(IVostokHostingEnvironment environment) => Task.CompletedTask;
        }
    }
}