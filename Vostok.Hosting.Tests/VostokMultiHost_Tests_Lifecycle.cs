using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Time;
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

            builder.SetupShutdownTimeout(1.Milliseconds());
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

            public async Task RunAsync(IVostokHostingEnvironment environment)
            {
                while (true)
                {
                    await Task.Delay(60 * 1000);
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