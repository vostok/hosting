using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Commons.Time;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.MultiHost;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.Tests
{
    [TestFixture]
    internal class VostokMultiHost_Tests_MultiHost
    {
        [Test]
        public void Should_stop_after_applications_finished()
        {
            var application = new VostokMultiHostApplicationSettings(
                new DelayApplication(),
                ("test", "test"),
                SetupMultiHostApplication);

            var vostokMultiHost = new VostokMultiHost(new VostokMultiHostSettings(SetupMultiHost), application);

            Action run = () => vostokMultiHost.RunAsync().GetAwaiter().GetResult();

            run.ShouldPassIn(1.Seconds());

            vostokMultiHost.MultiHostState.Should().Be(VostokMultiHostState.Exited);
        }

        [Test]
        public async Task Should_not_stop_if_contains_not_finished_application()
        {
            var application = new VostokMultiHostApplicationSettings(
                new DelayApplication(),
                ("test", "test"),
                SetupMultiHostApplication);

            var newApplication = new VostokMultiHostApplicationSettings(
                new NeverEndingApplication(),
                ("nevermind", "delay"),
                SetupMultiHostApplication);

            var vostokMultiHost = new VostokMultiHost(new VostokMultiHostSettings(SetupMultiHost), application);

            await vostokMultiHost.StartAsync();

            vostokMultiHost.AddApplication(newApplication);

            await Task.Delay(500);

            vostokMultiHost.MultiHostState.Should().Be(VostokMultiHostState.Running);

            await vostokMultiHost.StopAsync();
        }

        [Test]
        public async Task Should_not_stop_if_didnt_start_any_application()
        {
            var vostokMultiHost = new VostokMultiHost(new VostokMultiHostSettings(SetupMultiHost));

            await vostokMultiHost.StartAsync();

            await Task.Delay(500);

            vostokMultiHost.MultiHostState.Should().Be(VostokMultiHostState.Running);

            Action stop = () => vostokMultiHost.StopAsync().GetAwaiter().GetResult();

            stop.ShouldPassIn(1.Seconds());
        }

        private static void SetupMultiHost(IVostokHostingEnvironmentBuilder builder)
        {
            builder.SetupLog(log => log.SetupConsoleLog());
        }

        private static void SetupMultiHostApplication(IVostokHostingEnvironmentBuilder builder)
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

        private class NeverEndingApplication : IVostokApplication
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

        private class DelayApplication : IVostokApplication
        {
            public Task InitializeAsync(IVostokHostingEnvironment environment) =>
                Task.CompletedTask;

            public Task RunAsync(IVostokHostingEnvironment environment)
            {
                return Task.Delay(150);
            }
        }
    }
}