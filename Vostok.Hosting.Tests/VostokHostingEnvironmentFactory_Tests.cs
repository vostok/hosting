using System;
using System.Diagnostics;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Commons.Environment;
using Vostok.Commons.Testing;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Components.Shutdown;
using Vostok.Hosting.Components.ZooKeeper;
using Vostok.Hosting.Setup;
using Vostok.Metrics;
using Vostok.Metrics.System.Gc;
using Vostok.Metrics.System.Process;

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

            var before1 = environment.ShutdownTimeout;

            Thread.Sleep(100);

            var before2 = environment.ShutdownTimeout;

            before2.Should().Be(before1);

            shutdown.Cancel();

            var immediatelyAfter = environment.ShutdownTimeout;

            new Action(() =>
            {
                environment.ShutdownTimeout.Should().BeLessThan(immediatelyAfter);
            }).ShouldPassIn(5.Seconds());
        }

        [Test]
        public void Should_substract_beacon_shutdown_timeout_from_total()
        {
            var environment = VostokHostingEnvironmentFactory.Create(
                SetupWithServiceDiscovery,
                new VostokHostingEnvironmentFactorySettings
                {
                    BeaconShutdownTimeout = 3.Seconds()
                });

            environment.ShutdownTimeout.Should().Be(27.Seconds());
        }

        [Test]
        public void Should_not_substract_beacon_shutdown_timeout_from_total_if_there_is_no_real_beacon()
        {
            var environment = VostokHostingEnvironmentFactory.Create(
                Setup,
                new VostokHostingEnvironmentFactorySettings
                {
                    BeaconShutdownTimeout = 3.Seconds()
                });

            environment.ShutdownTimeout.Should().Be(30.Seconds());
        }

        [Test]
        public void Should_wait_after_beacon_shutdown_if_instructed_to_do_so()
        {
            var environment = VostokHostingEnvironmentFactory.Create(
                SetupWithServiceDiscovery,
                new VostokHostingEnvironmentFactorySettings
                {
                    BeaconShutdownTimeout = 1.Seconds()
                });

            var watch = Stopwatch.StartNew();

            shutdown.Cancel();

            environment.ShutdownToken.WaitHandle.WaitOne(5.Seconds()).Should().BeTrue();

            watch.Elapsed.Should().BeGreaterOrEqualTo(1.Seconds() - ShutdownConstants.CutAmountForBeaconTimeout - 50.Milliseconds());
        }

        [Test]
        public void Should_not_wait_after_beacon_shutdown_if_disabled()
        {
            var environment = VostokHostingEnvironmentFactory.Create(
                SetupWithServiceDiscovery,
                new VostokHostingEnvironmentFactorySettings
                {
                    BeaconShutdownTimeout = 2.Seconds(),
                    BeaconShutdownWaitEnabled = false
                });

            var watch = Stopwatch.StartNew();

            shutdown.Cancel();

            environment.ShutdownToken.WaitHandle.WaitOne(5.Seconds()).Should().BeTrue();

            watch.Elapsed.Should().BeLessThan(1.Seconds());
        }

        [Test]
        public void Should_provide_a_diagnostic_extension()
        {
            var environment = VostokHostingEnvironmentFactory.Create(Setup, new VostokHostingEnvironmentFactorySettings());

            environment.HostExtensions.TryGet<IVostokApplicationDiagnostics>(out var diagnostics).Should().BeTrue();

            diagnostics.Should().NotBeNull();
            diagnostics.Info.Should().NotBeNull();
            diagnostics.HealthTracker.Should().NotBeNull();
        }

        [Test]
        public void Should_provide_system_metrics_extensions()
        {
            var environment = VostokHostingEnvironmentFactory.Create(Setup, new VostokHostingEnvironmentFactorySettings());

            if (RuntimeDetector.IsDotNetCore30AndNewer)
                environment.HostExtensions.TryGet<GarbageCollectionMonitor>(out _).Should().BeTrue();

            environment.HostExtensions.TryGet<CurrentProcessMonitor>(out _).Should().BeTrue();
        }

        [Test]
        public void Should_provide_dev_null_metric_context_when_diagnostic_metrics_disabled_and_no_senders()
        {
            var environment = VostokHostingEnvironmentFactory.Create(Setup, new VostokHostingEnvironmentFactorySettings
            {
                DiagnosticMetricsEnabled = false
            });

            environment.Metrics.Root.Should().BeOfType<DevNullMetricContext>();
        }

        [Test]
        public void Should_provide_dev_null_metric_context_when_no_metric_event_senders_built()
        {
            var environment = VostokHostingEnvironmentFactory.Create(SetupForDevNullMetricContext, new VostokHostingEnvironmentFactorySettings());

            environment.Metrics.Root.Should().BeOfType<DevNullMetricContext>();

            void SetupForDevNullMetricContext(IVostokHostingEnvironmentBuilder builder)
            {
                Setup(builder);
                builder.SetupDiagnostics(x => x.CustomizeInfo(s => s.AddApplicationMetricsInfo = false)); // disable ApplicationMetricsProvider metrics sender
            }
        }

        [Test]
        public void Should_provide_not_dev_null_metric_context_when_metric_event_senders_built()
        {
            var environment = VostokHostingEnvironmentFactory.Create(SetupForDevNullMetricContext, new VostokHostingEnvironmentFactorySettings());

            environment.Metrics.Root.Should().BeOfType<MetricContext>();

            void SetupForDevNullMetricContext(IVostokHostingEnvironmentBuilder builder)
            {
                Setup(builder);
                builder.SetupDiagnostics(x => x.CustomizeInfo(s => s.AddApplicationMetricsInfo = true)); // enable ApplicationMetricsProvider metrics sender
            }
        }
        
        [Test, Explicit]
        public void Should_not_leak()
        {
            for (int i = 0; i < 100; i++)
            {
                var environment = VostokHostingEnvironmentFactory.Create(Setup,
                    new VostokHostingEnvironmentFactorySettings
                    {
                        ConfigureStaticProviders = false
                    });
                (environment as IDisposable)?.Dispose();
            }
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