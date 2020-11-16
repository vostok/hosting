using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Models;
using Vostok.Hosting.MultiHost;
using Vostok.Hosting.Setup;

// ReSharper disable PossibleNullReferenceException

namespace Vostok.Hosting.Tests
{
    [TestFixture]
    internal class VostokMultiHost_Tests_Applications
    {
        private VostokMultiHost vostokMultiHost;

        [TestCase(VostokApplicationState.EnvironmentSetup)]
        [TestCase(VostokApplicationState.EnvironmentWarmup)]
        [TestCase(VostokApplicationState.Initializing)]
        [TestCase(VostokApplicationState.Initialized)]
        [TestCase(VostokApplicationState.Running)]
        public async Task Start_should_wait_until_given_state_occurs(VostokApplicationState stateToAwait)
        {
            var identifier = new VostokMultiHostApplicationIdentifier("test", "test");
            var workerApplication = new VostokMultiHostApplicationSettings(new TestApplication(), identifier, SetupMultiHostApplication);

            vostokMultiHost = new VostokMultiHost(new VostokMultiHostSettings(SetupMultiHost), workerApplication);

            await vostokMultiHost.StartAsync();

            await vostokMultiHost.GetApplication(identifier).StartAsync(stateToAwait);

            vostokMultiHost.GetApplication(identifier).ApplicationState.Should().Match<VostokApplicationState>(state => state >= stateToAwait);

            await vostokMultiHost.StopApplication(identifier);

            vostokMultiHost.GetApplication(identifier).ApplicationState.IsTerminal().Should().BeTrue();

            await vostokMultiHost.StopAsync();
        }

        [Test]
        public async Task Start_and_stop_should_throw_on_initialize_failed()
        {
            await SetupAndStartMultiHost();

            var identifier = new VostokMultiHostApplicationIdentifier("test", "test");

            var badApplication = new VostokMultiHostApplicationSettings(new BadApplication(true), identifier, SetupMultiHostApplication);

            Action checkStart = () => vostokMultiHost.StartApplication(badApplication).GetAwaiter().GetResult();
            checkStart.Should().Throw<Exception>().WithMessage("initialize");

            Action checkStop = () => vostokMultiHost.StopApplication(identifier).GetAwaiter().GetResult();
            checkStop.Should().Throw<Exception>().WithMessage("initialize");
        }

        [Test]
        public async Task Start_should_not_throw_on_run_failed()
        {
            await SetupAndStartMultiHost();

            var identifier = new VostokMultiHostApplicationIdentifier("test", "test");

            var badApplication = new VostokMultiHostApplicationSettings(new BadApplication(false), identifier, SetupMultiHostApplication);

            Action checkStart = () => vostokMultiHost.StartApplication(badApplication).GetAwaiter().GetResult();
            checkStart.Should().NotThrow<Exception>();

            Action checkStop = () => vostokMultiHost.StopApplication(identifier).GetAwaiter().GetResult();
            checkStop.Should().Throw<Exception>().WithMessage("run");
        }

        [Test]
        public async Task Run_should_not_throw()
        {
            await SetupAndStartMultiHost();

            var identifier = new VostokMultiHostApplicationIdentifier("test", "test");

            var badApplication = new VostokMultiHostApplicationSettings(new BadApplication(false), identifier, SetupMultiHostApplication);

            Action checkStart = () => vostokMultiHost.RunApplication(badApplication).GetAwaiter().GetResult();
            checkStart.Should().NotThrow<Exception>();
        }

        [Test]
        public void Should_throw_on_add_if_VostokMultiHost_not_launched()
        {
            var identifier = new VostokMultiHostApplicationIdentifier("test", "test");

            var badApplication = new VostokMultiHostApplicationSettings(new BadApplication(false), identifier, SetupMultiHostApplication);

            vostokMultiHost = new VostokMultiHost(new VostokMultiHostSettings(SetupMultiHost));

            Action checkStart = () => vostokMultiHost.StartApplication(badApplication).GetAwaiter().GetResult();
            checkStart.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public async Task Should_throw_on_add_if_VostokMultiHost_is_stopped()
        {
            await SetupAndStartMultiHost();

            await vostokMultiHost.StopAsync();

            var identifier = new VostokMultiHostApplicationIdentifier("test", "test");

            var badApplication = new VostokMultiHostApplicationSettings(new BadApplication(false), identifier, SetupMultiHostApplication);

            Action checkStart = () => vostokMultiHost.RunApplication(badApplication).GetAwaiter().GetResult();

            checkStart.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public async Task Should_run_sequentially()
        {
            var currentIndex = 0;

            int GetIndex() => currentIndex;

            void SetIndex(int newIndex) => currentIndex = newIndex;

            await SetupAndStartMultiHost();

            var appList = new List<VostokMultiHostApplicationSettings>();

            for (var i = 1; i < 11; i++)
            {
                appList.Add(
                    new VostokMultiHostApplicationSettings(
                        new SequentialCheckerApplication(GetIndex, SetIndex, i),
                        new VostokMultiHostApplicationIdentifier("test", $"{i}"),
                        SetupMultiHostApplication)
                );
            }

            await vostokMultiHost.RunSequentially(appList);
        }

        [Test]
        public async Task Should_run_in_parallel()
        {
            await SetupAndStartMultiHost();

            var appList = new List<VostokMultiHostApplicationSettings>();

            for (var i = 1; i < 11; i++)
            {
                appList.Add(
                    new VostokMultiHostApplicationSettings(
                        new DelayApplication(),
                        new VostokMultiHostApplicationIdentifier("test", i.ToString()),
                        SetupMultiHostApplication)
                );
            }

            Action runInParallel = () => vostokMultiHost.RunInParallel(appList).GetAwaiter().GetResult();

            runInParallel.ShouldPassIn((150 * 2).Milliseconds());
        }

        [Test]
        public async Task Should_not_throw_on_second_start()
        {
            await SetupAndStartMultiHost();
            
            var workerApplication = new VostokMultiHostApplicationSettings(
                new DelayApplication(),
                new VostokMultiHostApplicationIdentifier("nevermind", "delay"),
                SetupMultiHostApplication);

            await vostokMultiHost.StartApplication(workerApplication);

            Action secondStart = () => vostokMultiHost.StartApplication(workerApplication).GetAwaiter().GetResult();
            
            secondStart.Should().NotThrow();
        }

        [Test]
        public async Task Should_return_same_result_on_second_run()
        {
            await SetupAndStartMultiHost();
            
            var workerApplication = new VostokMultiHostApplicationSettings(
                new DelayApplication(),
                new VostokMultiHostApplicationIdentifier("nevermind", "delay"),
                SetupMultiHostApplication);

            var result = await vostokMultiHost.RunApplication(workerApplication);

            var result2 = await vostokMultiHost.RunApplication(workerApplication);

            result.Should().BeSameAs(result2);
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

        private async Task SetupAndStartMultiHost()
        {
            var workerApplication = new VostokMultiHostApplicationSettings(
                new TestApplication(),
                new VostokMultiHostApplicationIdentifier("nevermind", "blabla"),
                SetupMultiHostApplication);

            vostokMultiHost = new VostokMultiHost(new VostokMultiHostSettings(SetupMultiHost), workerApplication);

            await vostokMultiHost.StartAsync();
        }

        private class SequentialCheckerApplication : IVostokApplication
        {
            private Func<int> getCounter;
            private Action<int> setCounter;
            private int index;

            public SequentialCheckerApplication(Func<int> getCounter, Action<int> setCounter, int index)
            {
                this.getCounter = getCounter;
                this.setCounter = setCounter;
                this.index = index;
            }

            public Task InitializeAsync(IVostokHostingEnvironment environment) => Task.CompletedTask;

            public Task RunAsync(IVostokHostingEnvironment environment)
            {
                getCounter.Should().Be(index - 1);
                setCounter(index);
                return Task.CompletedTask;
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

        private class BadApplication : IVostokApplication
        {
            private readonly bool failInitialize;

            public BadApplication(bool failInitialize) =>
                this.failInitialize = failInitialize;

            public Task InitializeAsync(IVostokHostingEnvironment environment)
                => failInitialize ? throw new Exception("initialize") : Task.CompletedTask;

            public Task RunAsync(IVostokHostingEnvironment environment)
                => throw new Exception("run");
        }
    }
}