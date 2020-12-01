using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Requirements;
using Vostok.Hosting.Models;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.ZooKeeper.Client;
using Vostok.ZooKeeper.Client.Abstractions;
using Vostok.ZooKeeper.Client.Abstractions.Model;
using Vostok.ZooKeeper.Client.Abstractions.Model.Authentication;
using Vostok.ZooKeeper.Client.Abstractions.Model.Request;
using Vostok.ZooKeeper.LocalEnsemble;

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

            Action checkStart = () => host.Start(VostokApplicationState.Initialized);
            checkStart.Should().Throw<Exception>().WithMessage("initialize");

            Action checkStop = () => host.Stop();
            checkStop.Should().Throw<Exception>().WithMessage("initialize");
        }

        [TestCase("badLogin", "password", VostokApplicationState.CrashedDuringRunning)]
        [TestCase("login", "password", VostokApplicationState.Exited)]
        public void Start_should_check_that_beacon_has_started(string zkLogin, string zkPassword, VostokApplicationState result)
        {
            var appPath = "/service-discovery/v2/dev/auth-test";
            var log = new SilentLog();

            using (var zk = ZooKeeperEnsemble.DeployNew(new ZooKeeperEnsembleSettings(), log))
            {
                var zkClient = new ZooKeeperClient(new ZooKeeperClientSettings(zk.ConnectionString), log);
                zkClient.Create(new CreateRequest(appPath, CreateMode.Persistent)).EnsureSuccess();
                zkClient.SetAcl(
                        new SetAclRequest(
                            appPath,
                            new List<Acl>
                            {
                                Acl.Digest(AclPermissions.All, "login", "password"),
                                Acl.ReadUnsafe
                            }))
                    .EnsureSuccess();

                zkClient.AddAuthenticationInfo(zkLogin, zkPassword);

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
                host.Start(VostokApplicationState.CrashedDuringRunning);
                host.ApplicationState.Should().Be(result);
            }
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