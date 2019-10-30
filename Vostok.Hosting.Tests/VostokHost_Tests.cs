using System;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Commons.Helpers.Extensions;
using Vostok.Commons.Testing.Observable;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Models;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.Tests
{
    [TestFixture]
    internal class VostokHost_Tests
    {
        private readonly TimeSpan shutdownTimeout = 1.Seconds();
        private SimpleApplication application;
        private VostokHost host;
        private TestObserver<VostokApplicationState> observer;
        private readonly Exception error = new Exception("Error.");
        private readonly Exception crashError = new Exception("Crashed.");

        [Test]
        public void Should_return_Exited()
        {
            CreateAndRunAsync(new SimpleApplicationSettings())
                .GetAwaiter()
                .GetResult()
                .State
                .Should()
                .Be(VostokApplicationState.Exited);

            CheckStates(
                VostokApplicationState.NotInitialized,
                VostokApplicationState.Initializing,
                VostokApplicationState.Initialized,
                VostokApplicationState.Running,
                VostokApplicationState.Exited);
        }

        [Test]
        public void Should_return_Stopped()
        {
            var runTask = CreateAndRunAsync(
                new SimpleApplicationSettings
                {
                    RunSleep = 10.Seconds()
                });

            CheckStates(
                VostokApplicationState.NotInitialized,
                VostokApplicationState.Initializing,
                VostokApplicationState.Initialized,
                VostokApplicationState.Running);

            host.ShutdownTokenSource.Cancel();

            runTask
                .GetAwaiter()
                .GetResult()
                .State
                .Should()
                .Be(VostokApplicationState.Stopped);

            CheckStates(
                VostokApplicationState.NotInitialized,
                VostokApplicationState.Initializing,
                VostokApplicationState.Initialized,
                VostokApplicationState.Running,
                VostokApplicationState.Stopping,
                VostokApplicationState.Stopped);
        }

        [Test]
        public void Should_return_StoppedForcibly()
        {
            var runTask = CreateAndRunAsync(
                new SimpleApplicationSettings
                {
                    RunSleep = 10.Seconds(),
                    GracefulShutdown = false
                });

            CheckStates(
                VostokApplicationState.NotInitialized,
                VostokApplicationState.Initializing,
                VostokApplicationState.Initialized,
                VostokApplicationState.Running);

            host.ShutdownTokenSource.Cancel();

            runTask
                .GetAwaiter()
                .GetResult()
                .State
                .Should()
                .Be(VostokApplicationState.StoppedForcibly);

            CheckStates(
                VostokApplicationState.NotInitialized,
                VostokApplicationState.Initializing,
                VostokApplicationState.Initialized,
                VostokApplicationState.Running,
                VostokApplicationState.Stopping,
                VostokApplicationState.StoppedForcibly);
        }

        [Test]
        public void Should_return_CrashedDuringInitialization()
        {
            var runTask = CreateAndRunAsync(
                new SimpleApplicationSettings
                {
                    CrashDuringInitializing = true
                });

            runTask
                .GetAwaiter()
                .GetResult()
                .State
                .Should()
                .Be(VostokApplicationState.CrashedDuringInitialization);

            CheckStates(
                crashError,
                VostokApplicationState.NotInitialized,
                VostokApplicationState.Initializing,
                VostokApplicationState.CrashedDuringInitialization);
        }

        [Test]
        public void Should_return_CrashedDuringRunning()
        {
            var runTask = CreateAndRunAsync(
                new SimpleApplicationSettings
                {
                    CrashDuringRunning = true
                });

            runTask
                .GetAwaiter()
                .GetResult()
                .State
                .Should()
                .Be(VostokApplicationState.CrashedDuringRunning);

            CheckStates(
                crashError,
                VostokApplicationState.NotInitialized,
                VostokApplicationState.Initializing,
                VostokApplicationState.Initialized,
                VostokApplicationState.Running,
                VostokApplicationState.CrashedDuringRunning);
        }

        [Test]
        public void Should_return_CrashedDuringStopping()
        {
            var runTask = CreateAndRunAsync(
                new SimpleApplicationSettings
                {
                    RunSleep = 10.Seconds(),
                    CrashDuringStopping = true
                });

            CheckStates(
                VostokApplicationState.NotInitialized,
                VostokApplicationState.Initializing,
                VostokApplicationState.Initialized,
                VostokApplicationState.Running);

            host.ShutdownTokenSource.Cancel();

            runTask
                .GetAwaiter()
                .GetResult()
                .State
                .Should()
                .Be(VostokApplicationState.CrashedDuringStopping);

            CheckStates(
                crashError,
                VostokApplicationState.NotInitialized,
                VostokApplicationState.Initializing,
                VostokApplicationState.Initialized,
                VostokApplicationState.Running,
                VostokApplicationState.Stopping,
                VostokApplicationState.CrashedDuringStopping);
        }

        [Test]
        public void Should_throw_InvalidOperationException_on_second_run()
        {
            CreateAndRunAsync(new SimpleApplicationSettings())
                .GetAwaiter()
                .GetResult()
                .State
                .Should()
                .Be(VostokApplicationState.Exited);

            ((Action)(() => host.RunAsync().GetAwaiter().GetResult()))
                .Should()
                .Throw<InvalidOperationException>();
        }

        [Test]
        public void Should_throw_Exception_from_environment_builder()
        {
            application = new SimpleApplication(new SimpleApplicationSettings());

            VostokHostingEnvironmentSetup environmentSetup = setup =>
            {
                setup
                    .SetupApplicationIdentity(
                        (applicationIdentitySetup, setupContext) => applicationIdentitySetup
                            .SetProject("Infrastructure")
                            .SetSubproject("vostok")
                            .SetEnvironment("dev")
                            .SetApplication("simple-application")
                            .SetInstance("1")
                    )
                    .SetupLog(
                        logSetup => throw error);
            };

            host = new VostokHost(
                new VostokHostSettings(application, environmentSetup)
                {
                    ShutdownTimeout = shutdownTimeout
                });

            ((Action)(() => host.Run())).Should().Throw<Exception>().Which.Should().Be(error);
        }

        private void CheckStates(params VostokApplicationState[] states)
        {
            CheckStates(null, states);
        }

        private void CheckStates(Exception e, params VostokApplicationState[] states)
        {
            var notifications = states.Select(Notification.CreateOnNext).ToList();
            if (e != null)
                notifications.Add(Notification.CreateOnError<VostokApplicationState>(e));

            observer.Messages.Should().BeEquivalentTo(notifications, options => options.WithStrictOrdering());
        }

        private Task<VostokApplicationRunResult> CreateAndRunAsync(SimpleApplicationSettings settings)
        {
            observer = new TestObserver<VostokApplicationState>();

            settings.CrashError = crashError;
            application = new SimpleApplication(settings);

            VostokHostingEnvironmentSetup environmentSetup = setup =>
            {
                setup
                    .SetupApplicationIdentity(
                        (applicationIdentitySetup, setupContext) => applicationIdentitySetup
                            .SetProject("Infrastructure")
                            .SetSubproject("vostok")
                            .SetEnvironment("dev")
                            .SetApplication("simple-application")
                            .SetInstance("1")
                    )
                    .SetupLog(
                        logSetup =>
                            logSetup
                                .SetupConsoleLog(
                                    consoleLogSetup =>
                                        consoleLogSetup.UseSynchronous()));
            };

            host = new VostokHost(
                new VostokHostSettings(application, environmentSetup)
                {
                    ShutdownTimeout = shutdownTimeout
                });

            host.OnApplicationStateChanged.Subscribe(observer);

            return host.RunAsync();
        }

        private class SimpleApplicationSettings
        {
            public TimeSpan InitializeSleep { get; set; }
            public TimeSpan RunSleep { get; set; }
            public bool CrashDuringInitializing { get; set; }
            public bool CrashDuringRunning { get; set; }
            public bool CrashDuringStopping { get; set; }
            public bool GracefulShutdown { get; set; } = true;
            public Exception CrashError { get; set; }
        }

        private class SimpleApplication : IVostokApplication
        {
            private readonly SimpleApplicationSettings settings;

            public SimpleApplication(SimpleApplicationSettings settings)
            {
                this.settings = settings;
            }

            public async Task InitializeAsync(IVostokHostingEnvironment environment)
            {
                environment.Log.Info("Hello from initialize!");

                if (settings.CrashDuringInitializing)
                    throw settings.CrashError;

                await Task.Delay(settings.InitializeSleep);
            }

            public async Task RunAsync(IVostokHostingEnvironment environment)
            {
                environment.Log.Info("Hello again from run!");

                if (settings.CrashDuringRunning)
                    throw settings.CrashError;

                if (settings.CrashDuringStopping)
                {
                    await Task.Delay(settings.RunSleep, environment.ShutdownToken).SilentlyContinue();
                    throw settings.CrashError;
                }

                if (settings.GracefulShutdown)
                    await Task.Delay(settings.RunSleep, environment.ShutdownToken).SilentlyContinue();
                else
                    await Task.Delay(settings.RunSleep);
            }
        }
    }
}