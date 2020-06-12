using System;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Commons.Testing;
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

            assertion.ShouldPassIn(10.Seconds());
        }

        [Test]
        public void Should_produce_an_environment_with_ticking_shutdown_budget_upon_cancellation()
        {
            var environment = VostokHostingEnvironmentFactory.Create(Setup);

            shutdown.Cancel();

            var before = environment.ShutdownTimeout;

            Thread.Sleep(50);

            var after = environment.ShutdownTimeout;

            after.Should().BeLessThan(before);
        }

        private void Setup(IVostokHostingEnvironmentBuilder builder)
        {
            builder.DisableClusterConfig();
            builder.DisableZooKeeper();
            builder.DisableHercules();

            builder.SetupApplicationIdentity(
                id =>
                {
                    id.SetProject("Project");
                    id.SetApplication("App");
                    id.SetEnvironment("Environment");
                    id.SetInstance("Instance");
                });

            builder.SetupShutdownToken(shutdown.Token);
        }
    }
}
