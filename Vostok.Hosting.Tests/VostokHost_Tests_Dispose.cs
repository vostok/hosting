using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Time;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Models;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.Tests
{
    [TestFixture]
    internal class VostokHost_Tests_Dispose
    {
        [Test]
        public void Should_dispose_of_the_application()
        {
            var app = new DisposableApplication();

            var host = new VostokHost(new TestHostSettings(app, SetupEnvironment));

            host.Run().State.Should().Be(VostokApplicationState.Exited);

            app.Disposed.Should().BeTrue();
        }

        [Test]
        public void Should_not_crash_on_dispose_errors()
        {
            var app = new DisposableApplication
            {
                DisposeError = new Exception("crash")
            };

            var host = new VostokHost(new TestHostSettings(app, SetupEnvironment));

            host.Run().State.Should().Be(VostokApplicationState.Exited);

            app.Disposed.Should().BeTrue();
        }

        [Test]
        public void Should_not_block_on_dispose_longer_than_shutdown_timeout_allows()
        {
            var app = new DisposableApplication
            {
                DisposeDelay = 10.Seconds()
            };

            var host = new VostokHost(new TestHostSettings(app, SetupEnvironment));

            host.Start();

            var watch = Stopwatch.StartNew();

            host.Stop().State.Should().Be(VostokApplicationState.Exited);

            watch.Elapsed.Should().BeLessThan(2.Seconds());
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

            builder.SetupShutdownTimeout(1.Seconds());
        }

        private class DisposableApplication : IVostokApplication, IDisposable
        {
            public TimeSpan? DisposeDelay { get; set; }

            public Exception DisposeError { get; set; }

            public bool Disposed { get; private set; }

            public void Dispose()
            {
                Disposed = true;

                if (DisposeError != null)
                    throw DisposeError;

                if (DisposeDelay.HasValue)
                    Thread.Sleep(DisposeDelay.Value);
            }

            public Task InitializeAsync(IVostokHostingEnvironment environment) =>
                Task.CompletedTask;

            public Task RunAsync(IVostokHostingEnvironment environment) =>
                Task.CompletedTask;
        }
    }
}
