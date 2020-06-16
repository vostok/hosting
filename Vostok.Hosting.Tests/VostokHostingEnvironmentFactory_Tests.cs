using System;
using System.Diagnostics;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Hosting.Components.Shutdown;
using Vostok.Hosting.Components.ZooKeeper;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.Tests
{
    [TestFixture]
    internal class VostokHostingEnvironmentFactory_Tests
    {
        private CancellationTokenSource shutdown;

        [SetUp]
        public void TestSetup()
        {
            shutdown = new CancellationTokenSource();
        }

        [Test]
        public void Should_produce_an_environment_with_linked_cancellation_tokens()
        {
            var environment = VostokHostingEnvironmentFactory.Create(Setup);
            
            shutdown.Cancel();

            Action assertion = () => environment.ShutdownToken.IsCancellationRequested.Should().BeTrue();

            assertion.ShouldPassIn(2.Seconds());
        }

        [Test]
        public void Should_produce_an_environment_with_ticking_shutdown_budget_upon_cancellation()
        {
            var environment = VostokHostingEnvironmentFactory.Create(Setup);

            shutdown.Cancel();

            var before = environment.ShutdownTimeout;

            Thread.Sleep(100);

            var after = environment.ShutdownTimeout;

            after.Should().BeLessThan(before);
        }

        [Test]
        public void Should_substract_beacon_shutdown_timeout_from_total()
        {
            var environment = VostokHostingEnvironmentFactory.Create(SetupWithServiceDiscovery, new VostokHostingEnvironmentFactorySettings
            {
                BeaconShutdownTimeout = 3.Seconds()
            });

            environment.ShutdownTimeout.Should().Be(27.Seconds());
        }

        [Test]
        public void Should_not_substract_beacon_shutdown_timeout_from_total_if_there_is_no_real_beacon()
        {
            var environment = VostokHostingEnvironmentFactory.Create(Setup, new VostokHostingEnvironmentFactorySettings
            {
                BeaconShutdownTimeout = 3.Seconds()
            });

            environment.ShutdownTimeout.Should().Be(30.Seconds());
        }

        [Test]
        public void Should_wait_after_beacon_shutdown_if_instructed_to_do_so()
        {
            var environment = VostokHostingEnvironmentFactory.Create(SetupWithServiceDiscovery, new VostokHostingEnvironmentFactorySettings
            {
                BeaconShutdownTimeout = 1.Seconds()
            });

            var watch = Stopwatch.StartNew();

            shutdown.Cancel();

            environment.ShutdownToken.WaitHandle.WaitOne(5.Seconds()).Should().BeTrue();

            watch.Elapsed.Should().BeGreaterOrEqualTo(1.Seconds() - ShutdownConstants.CutAmountForBeaconTimeout);
        }

        [Test]
        public void Should_not_wait_after_beacon_shutdown_if_disabled()
        {
            var environment = VostokHostingEnvironmentFactory.Create(SetupWithServiceDiscovery, new VostokHostingEnvironmentFactorySettings
            {
                BeaconShutdownTimeout = 2.Seconds(),
                BeaconShutdownWaitEnabled = false
            });

            var watch = Stopwatch.StartNew();

            shutdown.Cancel();

            environment.ShutdownToken.WaitHandle.WaitOne(5.Seconds()).Should().BeTrue();

            watch.Elapsed.Should().BeLessThan(1.Seconds());
        }

        private void Setup(IVostokHostingEnvironmentBuilder builder)
        {
            builder.DisableClusterConfig();
            builder.DisableZooKeeper();
            builder.DisableHercules();

            SetupCommons(builder);
        }

        private void SetupWithServiceDiscovery(IVostokHostingEnvironmentBuilder builder)
        {
            builder.SetPort(1234);

            builder.SetupZooKeeperClient(zk => zk.UseInstance(new DevNullZooKeeperClient()));

            SetupCommons(builder);
        }

        private void SetupCommons(IVostokHostingEnvironmentBuilder builder)
        {
            builder.SetupApplicationIdentity(
                id =>
                {
                    id.SetProject("Project");
                    id.SetApplication("App");
                    id.SetEnvironment("Environment");
                    id.SetInstance("Instance");
                });

            builder.SetupShutdownToken(shutdown.Token);

            builder.SetupShutdownTimeout(30.Seconds() + ShutdownConstants.CutAmountForExternalTimeout);

            builder.SetupLog(log => log.SetupConsoleLog());
        }
    }
}
