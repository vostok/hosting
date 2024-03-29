﻿using System;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using JetBrains.Annotations;
using NUnit.Framework;
using Vostok.Commons.Helpers.Extensions;
using Vostok.Commons.Testing;
using Vostok.Commons.Testing.Observable;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Models;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.Tests
{
    [TestFixture]
    internal class VostokHost_Tests_Lifecycle
    {
        private readonly TimeSpan shutdownTimeout = 1.Seconds();
        private readonly Exception error = new Exception("Error.");
        private readonly Exception crashError = new Exception("Crashed.");
        private SimpleApplication application;
        private VostokHost host;
        private TestObserver<VostokApplicationState> observer;

        [Test]
        public void Should_return_Exited()
        {
            CreateAndRunAsync(new SimpleApplicationSettings())
                .GetAwaiter()
                .GetResult()
                .State
                .Should()
                .Be(VostokApplicationState.Exited);

            CheckStatesCompleted(
                VostokApplicationState.NotInitialized,
                VostokApplicationState.EnvironmentSetup,
                VostokApplicationState.EnvironmentWarmup,
                VostokApplicationState.Initializing,
                VostokApplicationState.Initialized,
                VostokApplicationState.Running,
                VostokApplicationState.Exited);
        }

        [Test]
        public void Should_return_Stopped_initializing()
        {
            var runTask = CreateAndRunAsync(
                new SimpleApplicationSettings
                {
                    InitializeSleep = 10.Seconds()
                });

            CheckStates(
                VostokApplicationState.NotInitialized,
                VostokApplicationState.EnvironmentSetup,
                VostokApplicationState.EnvironmentWarmup,
                VostokApplicationState.Initializing);

            host.ShutdownTokenSource.Cancel();

            runTask
                .GetAwaiter()
                .GetResult()
                .State
                .Should()
                .Be(VostokApplicationState.Stopped);

            CheckStatesCompleted(
                VostokApplicationState.NotInitialized,
                VostokApplicationState.EnvironmentSetup,
                VostokApplicationState.EnvironmentWarmup,
                VostokApplicationState.Initializing,
                VostokApplicationState.Stopping,
                VostokApplicationState.Stopped);
        }

        [Test]
        public void Should_return_Stopped_running()
        {
            var runTask = CreateAndRunAsync(
                new SimpleApplicationSettings
                {
                    RunSleep = 10.Seconds()
                });

            CheckStates(
                VostokApplicationState.NotInitialized,
                VostokApplicationState.EnvironmentSetup,
                VostokApplicationState.EnvironmentWarmup,
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

            CheckStatesCompleted(
                VostokApplicationState.NotInitialized,
                VostokApplicationState.EnvironmentSetup,
                VostokApplicationState.EnvironmentWarmup,
                VostokApplicationState.Initializing,
                VostokApplicationState.Initialized,
                VostokApplicationState.Running,
                VostokApplicationState.Stopping,
                VostokApplicationState.Stopped);
        }

        [Test]
        public void Should_return_StoppedForcibly_initializing()
        {
            var runTask = CreateAndRunAsync(
                new SimpleApplicationSettings
                {
                    InitializeSleep = 10.Seconds(),
                    GracefulShutdown = false
                });

            CheckStates(
                VostokApplicationState.NotInitialized,
                VostokApplicationState.EnvironmentSetup,
                VostokApplicationState.EnvironmentWarmup,
                VostokApplicationState.Initializing);

            host.ShutdownTokenSource.Cancel();

            runTask
                .GetAwaiter()
                .GetResult()
                .State
                .Should()
                .Be(VostokApplicationState.StoppedForcibly);

            CheckStatesCompleted(
                VostokApplicationState.NotInitialized,
                VostokApplicationState.EnvironmentSetup,
                VostokApplicationState.EnvironmentWarmup,
                VostokApplicationState.Initializing,
                VostokApplicationState.Stopping,
                VostokApplicationState.StoppedForcibly);
        }

        [Test]
        public void Should_return_StoppedForcibly_running()
        {
            var runTask = CreateAndRunAsync(
                new SimpleApplicationSettings
                {
                    RunSleep = 10.Seconds(),
                    GracefulShutdown = false
                });

            CheckStates(
                VostokApplicationState.NotInitialized,
                VostokApplicationState.EnvironmentSetup,
                VostokApplicationState.EnvironmentWarmup,
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

            CheckStatesCompleted(
                VostokApplicationState.NotInitialized,
                VostokApplicationState.EnvironmentSetup,
                VostokApplicationState.EnvironmentWarmup,
                VostokApplicationState.Initializing,
                VostokApplicationState.Initialized,
                VostokApplicationState.Running,
                VostokApplicationState.Stopping,
                VostokApplicationState.StoppedForcibly);
        }

        [Test]
        public void Should_return_CrashedDuringEnvironmentSetup()
        {
            var runTask = CreateAndRunAsync(new SimpleApplicationSettings(), builder => builder.SetupDatacenters(_ => throw error));

            runTask
                .GetAwaiter()
                .GetResult()
                .State
                .Should()
                .Be(VostokApplicationState.CrashedDuringEnvironmentSetup);

            CheckStates(
                error,
                VostokApplicationState.NotInitialized,
                VostokApplicationState.EnvironmentSetup,
                VostokApplicationState.CrashedDuringEnvironmentSetup);
        }

        [Test]
        public void Should_return_CrashedDuringEnvironmentWarmup()
        {
            var runTask = CreateAndRunAsync(new SimpleApplicationSettings(), additionalHostSetup: settings => { settings.BeforeInitializeApplication.Add(_ => throw error); });

            runTask
                .GetAwaiter()
                .GetResult()
                .State
                .Should()
                .Be(VostokApplicationState.CrashedDuringEnvironmentWarmup);

            CheckStates(
                error,
                VostokApplicationState.NotInitialized,
                VostokApplicationState.EnvironmentSetup,
                VostokApplicationState.EnvironmentWarmup,
                VostokApplicationState.CrashedDuringEnvironmentWarmup);
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
                VostokApplicationState.EnvironmentSetup,
                VostokApplicationState.EnvironmentWarmup,
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
                VostokApplicationState.EnvironmentSetup,
                VostokApplicationState.EnvironmentWarmup,
                VostokApplicationState.Initializing,
                VostokApplicationState.Initialized,
                VostokApplicationState.Running,
                VostokApplicationState.CrashedDuringRunning);
        }

        [Test]
        public void Should_return_CrashedDuringStopping_initializing()
        {
            var runTask = CreateAndRunAsync(
                new SimpleApplicationSettings
                {
                    InitializeSleep = 10.Seconds(),
                    CrashDuringStopping = true
                });

            CheckStates(
                VostokApplicationState.NotInitialized,
                VostokApplicationState.EnvironmentSetup,
                VostokApplicationState.EnvironmentWarmup,
                VostokApplicationState.Initializing);

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
                VostokApplicationState.EnvironmentSetup,
                VostokApplicationState.EnvironmentWarmup,
                VostokApplicationState.Initializing,
                VostokApplicationState.Stopping,
                VostokApplicationState.CrashedDuringStopping);
        }

        [Test]
        public void Should_return_CrashedDuringStopping_running()
        {
            var runTask = CreateAndRunAsync(
                new SimpleApplicationSettings
                {
                    RunSleep = 10.Seconds(),
                    CrashDuringStopping = true
                });

            CheckStates(
                VostokApplicationState.NotInitialized,
                VostokApplicationState.EnvironmentSetup,
                VostokApplicationState.EnvironmentWarmup,
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
                VostokApplicationState.EnvironmentSetup,
                VostokApplicationState.EnvironmentWarmup,
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

        private void CheckStates(params VostokApplicationState[] states)
            => CheckStates(null, states);

        private void CheckStatesCompleted(params VostokApplicationState[] states)
            => CheckStates(null, true, states);

        private void CheckStates(Exception e, params VostokApplicationState[] states)
            => CheckStates(e, false, states);

        private void CheckStates(Exception e, bool completed, params VostokApplicationState[] states)
        {
            ((Action)(() => host.ApplicationState.Should().Be(states.Last())))
                .ShouldPassIn(1.Seconds());

            var notifications = states.Select(Notification.CreateOnNext).ToList();
            if (e != null)
                notifications.Add(Notification.CreateOnError<VostokApplicationState>(e));
            if (completed)
                notifications.Add(Notification.CreateOnCompleted<VostokApplicationState>());

            observer.Messages.Should().BeEquivalentTo(notifications, options => options.WithStrictOrdering());
        }

        private Task<VostokApplicationRunResult> CreateAndRunAsync(
            [NotNull] SimpleApplicationSettings settings,
            [CanBeNull] Action<IVostokHostingEnvironmentBuilder> additionalEnvSetup = null,
            [CanBeNull] Action<VostokHostSettings> additionalHostSetup = null)
        {
            observer = new TestObserver<VostokApplicationState>();

            settings.CrashError = crashError;
            application = new SimpleApplication(settings);

            void EnvironmentSetup(IVostokHostingEnvironmentBuilder builder)
            {
                builder.SetupApplicationIdentity(
                        (applicationIdentitySetup, setupContext) => applicationIdentitySetup.SetProject("Infrastructure")
                            .SetSubproject("vostok")
                            .SetEnvironment("dev")
                            .SetApplication("simple-application")
                            .SetInstance("1"))
                    .SetupLog(logSetup => logSetup.SetupConsoleLog(consoleLogSetup => consoleLogSetup.UseSynchronous()));

                additionalEnvSetup?.Invoke(builder);
            }

            var hostSettings = new TestHostSettings(application, EnvironmentSetup)
            {
                ShutdownTimeout = shutdownTimeout
            };

            additionalHostSetup?.Invoke(hostSettings);

            host = new VostokHost(hostSettings);

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

                await SleepAsync(environment, settings.InitializeSleep);
            }

            public async Task RunAsync(IVostokHostingEnvironment environment)
            {
                environment.Log.Info("Hello again from run!");

                if (settings.CrashDuringRunning)
                    throw settings.CrashError;

                await SleepAsync(environment, settings.RunSleep);
            }

            private async Task SleepAsync(IVostokHostingEnvironment environment, TimeSpan sleep)
            {
                if (settings.GracefulShutdown || settings.CrashDuringStopping)
                {
                    await Task.Delay(sleep, environment.ShutdownToken).SilentlyContinue();
                    if (settings.CrashDuringStopping && environment.ShutdownToken.IsCancellationRequested)
                        throw settings.CrashError;
                }
                else
                    await Task.Delay(sleep);
            }
        }
    }
}