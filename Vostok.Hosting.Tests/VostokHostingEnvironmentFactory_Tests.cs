using System.Threading;
using FluentAssertions;
using NUnit.Framework;
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
        public void Should_produce_an_environment_with_linked_cancellation_tokens_by_default()
        {
            var environment = VostokHostingEnvironmentFactory.Create(Setup);
            
            shutdown.Cancel();

            environment.ShutdownToken.IsCancellationRequested.Should().BeTrue();
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
