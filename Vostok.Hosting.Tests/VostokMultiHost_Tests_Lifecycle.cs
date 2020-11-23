using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.MultiHost;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.Tests
{
    [TestFixture]
    internal class VostokMultiHost_Tests_Lifecycle
    {
        private VostokMultiHost vostokMultiHost;

        [Test]
        public void Should_return_CrashedDuringEnvironmentSetup()
        {
            vostokMultiHost = new VostokMultiHost(
                new VostokMultiHostSettings(
                    builder =>
                    {
                        builder.SetupApplicationIdentity(identityBuilder => throw new Exception("Failed!"));
                        builder.SetupLog(logBuilder => logBuilder.SetupConsoleLog());
                    })
            );

            Action error = () => vostokMultiHost.RunAsync().GetAwaiter().GetResult();

            error.Should().Throw<Exception>().WithMessage("Failed!");

            vostokMultiHost.MultiHostState.Should().Be(VostokMultiHostState.CrashedDuringEnvironmentSetup);
        }

        [Test]
        public async Task Should_return_CrashedDuringStopping()
        {
            var workerApplication = new VostokMultiHostApplicationSettings(
                new TestApplication(),
                ("test", "test"),
                builder => builder.SetupLog(logBuilder => logBuilder.SetupConsoleLog()));

            vostokMultiHost = new VostokMultiHost(
                new VostokMultiHostSettings(
                    builder =>
                    {
                        // By the way, it's just irrational to use AddDisposable in VostokMultiHost :(

                        builder.SetupHostExtensions(extensionsBuilder => extensionsBuilder.AddDisposable(new BadDisposable()));
                        builder.SetupLog(logBuilder => logBuilder.SetupConsoleLog());
                    }),
                workerApplication
            );

            await vostokMultiHost.StartAsync();

            var result = await vostokMultiHost.StopAsync();

            result.State.Should().Be(VostokMultiHostState.CrashedDuringStopping);

            Action error = () => result.EnsureSuccess();

            error.Should().Throw<Exception>().WithMessage("HAHA! FAILED!");

            vostokMultiHost.MultiHostState.Should().Be(VostokMultiHostState.CrashedDuringStopping);
        }

        [Test]
        public async Task Should_return_exited()
        {
            SetupMultiHost();

            await vostokMultiHost.StartAsync();

            var result = await vostokMultiHost.StopAsync();

            result.State.Should().Be(VostokMultiHostState.Exited);

            result.Error.Should().Be(null);

            vostokMultiHost.MultiHostState.Should().Be(VostokMultiHostState.Exited);
        }

        [Test]
        public async Task Should_return_NotInitialized_on_stop_before_start()
        {
            SetupMultiHost();

            var result = await vostokMultiHost.StopAsync();

            result.State.Should().Be(VostokMultiHostState.NotInitialized);
        }

        [Test]
        public async Task Should_not_throw_on_second_launch()
        {
            SetupMultiHost();

            await vostokMultiHost.StartAsync();

            Action secondLaunch = () => vostokMultiHost.StartAsync().GetAwaiter().GetResult();

            secondLaunch.Should().NotThrow();
        }

        private static void MultiHostBuilder(IVostokHostingEnvironmentBuilder builder)
        {
            builder.SetupLog(log => log.SetupConsoleLog());
        }

        private static void MultiHostApplicationBuilder(IVostokHostingEnvironmentBuilder builder)
        {
            builder.SetupApplicationIdentity(
                id =>
                {
                    id.SetProject("good project");
                    id.SetApplication("vostok-test");
                    id.SetEnvironment("dev");
                    id.SetInstance("the only one");
                });

            builder.SetupLog(log => log.SetupConsoleLog());
        }

        private void SetupMultiHost()
        {
            var workerApplication = new VostokMultiHostApplicationSettings(
                new TestApplication(),
                ("nevermind", "blabla"),
                MultiHostApplicationBuilder);

            vostokMultiHost = new VostokMultiHost(new VostokMultiHostSettings(MultiHostBuilder), workerApplication);
        }

        private class TestApplication : IVostokApplication
        {
            public Task InitializeAsync(IVostokHostingEnvironment environment) => Task.Delay(150);

            public Task RunAsync(IVostokHostingEnvironment environment)
            {
                while (true)
                {
                }

                // ReSharper disable once FunctionNeverReturns
            }
        }

        private class BadDisposable : IDisposable
        {
            public void Dispose()
            {
                throw new Exception("HAHA! FAILED!");
            }
        }
    }
}