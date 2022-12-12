using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Helpers.Disposable;
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
        public void Should_dispose_components_with_reverse_order()
        {
            var check = "";
            var app = new DisposableApplication();
            var component1 = new ActionDisposable(() => check += "1");
            var component2 = new ActionDisposable(() => check += "2");

            var host = new VostokHost(new TestHostSettings(app,
                setup =>
                {
                    SetupEnvironment(setup);
                    setup.SetupHostExtensions(e => e.AddDisposable("2", component2));
                    setup.SetupHostExtensions(e => e.AddDisposable("1", component1));
                }));

            host.Run().State.Should().Be(VostokApplicationState.Exited);

            app.Disposed.Should().BeTrue();
            check.Should().Be("12");
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
        public void Should_not_crash_on_components_dispose_errors()
        {
            var check = "";
            var app = new DisposableApplication();
            var component1 = new ActionDisposable(() => throw new Exception("crash"));
            var component2 = new ActionDisposable(() => check += "2");

            var host = new VostokHost(new TestHostSettings(app,
                setup =>
                {
                    SetupEnvironment(setup);
                    setup.SetupHostExtensions(e => e.AddDisposable("2", component2));
                    setup.SetupHostExtensions(e => e.AddDisposable("1", component1));
                }));

            host.Run().State.Should().Be(VostokApplicationState.Exited);

            check.Should().Be("2");
        }

        [Test]
        public void Should_not_block_on_dispose_longer_than_shutdown_timeout_allows()
        {
            var app = new DisposableApplication
            {
                DisposeDelay = 10.Seconds()
            };

            var host = new VostokHost(new TestHostSettings(app, SetupEnvironment));

            var watch = Stopwatch.StartNew();

            host.Run().State.Should().Be(VostokApplicationState.Exited);

            watch.Elapsed.Should().BeLessThan(5.Seconds());
        }

        [Test]
        public void Should_not_block_on_components_dispose_longer_than_dispose_timeout_allows()
        {
            var app = new DisposableApplication();
            var component = new ActionDisposable(() => Thread.Sleep(10.Seconds()));

            var host = new VostokHost(new TestHostSettings(app,
                setup =>
                {
                    SetupEnvironment(setup);
                    setup.SetupHostExtensions(e => e.AddDisposable(component));
                    setup.SetupShutdownTimeout(1.Minutes());
                })
            {
                DisposeComponentTimeout = 1.Seconds()
            });

            var watch = Stopwatch.StartNew();

            host.Run().State.Should().Be(VostokApplicationState.Exited);

            watch.Elapsed.Should().BeLessThan(5.Seconds());
        }

        [Test]
        public void Should_not_have_any_references_on_app_or_host_after_dispose()
        {
            var (app, host) = RunApp();

            GC.Collect(2, GCCollectionMode.Forced, true, true);

            app.IsAlive.Should().BeFalse();
            host.IsAlive.Should().BeFalse();
        }

        private static (WeakReference, WeakReference) RunApp()
        {
            var app = new DisposableApplication();
            var host = new VostokHost(new TestHostSettings(app, SetupEnvironment) {ConfigureStaticProviders = false});

            var appWeak = new WeakReference(app);
            var hostWeak = new WeakReference(host);

            host.Run().State.Should().Be(VostokApplicationState.Exited);

            return (appWeak, hostWeak);
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