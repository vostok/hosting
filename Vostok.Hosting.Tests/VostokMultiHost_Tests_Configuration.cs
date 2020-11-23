using System;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.MultiHost;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.Tests
{
    [TestFixture]
    internal class VostokMultiHost_Tests_Configuration
    {
        [Test]
        public void Should_interpret_MultiHost_builder_settings_as_default_in_applications()
        {
            void Assertion(IVostokHostingEnvironment environment) => environment.HostExtensions.Get<TestHostExtension>().A.Should().Be(10);

            var vostokMultiHost = new VostokMultiHost(
                new VostokMultiHostSettings(
                    builder =>
                    {
                        builder.SetupLog(logBuilder => logBuilder.SetupConsoleLog());
                        builder.SetupHostExtensions(extensionsBuilder => extensionsBuilder.Add(new TestHostExtension(10)));
                    }));

            var application = new VostokMultiHostApplicationSettings(
                new TesterApplication(Assertion),
                ("test", "test"),
                SetupMultiHostApplication);

            vostokMultiHost.AddApplication(application);

            Action run = () => vostokMultiHost.RunAsync().GetAwaiter().GetResult().EnsureSuccess();

            run.Should().NotThrow();
        }

        [Test]
        public void Should_configure_application_and_instance_from_application_identifier()
        {
            void Assertion(IVostokHostingEnvironment environment)
            {
                environment.ApplicationIdentity.Project.Should().Be("testProject");
                environment.ApplicationIdentity.Environment.Should().Be("testEnvironment");
                environment.ApplicationIdentity.Application.Should().Be("testApplication");
                environment.ApplicationIdentity.Instance.Should().Be("testInstance");
            }

            var vostokMultiHost = new VostokMultiHost(
                new VostokMultiHostSettings(
                    builder =>
                    {
                        builder.SetupApplicationIdentity(identityBuilder => identityBuilder.SetProject("testProject").SetEnvironment("testEnvironment"));
                        builder.SetupLog(logBuilder => logBuilder.SetupConsoleLog());
                    }));

            var application = new VostokMultiHostApplicationSettings(
                new TesterApplication(Assertion),
                ("testApplication", "testInstance"),
                SetupMultiHostApplication);

            vostokMultiHost.AddApplication(application);

            Action run = () => vostokMultiHost.RunAsync().GetAwaiter().GetResult().EnsureSuccess();

            run.Should().NotThrow();
        }

        [Test]
        public void Should_override_default_host_settings()
        {
            void Assertion(IVostokHostingEnvironment environment)
            {
                environment.ApplicationIdentity.Project.Should().Be("testProject");
                environment.ApplicationIdentity.Environment.Should().Be("testEnvironment");
                environment.ApplicationIdentity.Application.Should().Be("testApplication");
                environment.ApplicationIdentity.Instance.Should().Be("testInstance");
            }

            var vostokMultiHost = new VostokMultiHost(
                new VostokMultiHostSettings(
                    builder =>
                    {
                        builder.SetupApplicationIdentity(
                            identityBuilder => { identityBuilder.SetProject("testProject").SetEnvironment("testEnvironment").SetApplication("default"); });
                        builder.SetupLog(logBuilder => logBuilder.SetupConsoleLog());
                    }));

            var application = new VostokMultiHostApplicationSettings(
                new TesterApplication(Assertion),
                ("testApplication", "testInstance"),
                SetupMultiHostApplication);

            vostokMultiHost.AddApplication(application);

            Action run = () => vostokMultiHost.RunAsync().GetAwaiter().GetResult().EnsureSuccess();

            run.Should().NotThrow();
        }

        [Test]
        public async Task Should_use_common_components()
        {
            IHerculesSink extractedSink = null;

            void Assert(IVostokHostingEnvironment environment)
            {
                environment.HerculesSink.Should().BeSameAs(extractedSink);
            }

            void Extract(IVostokHostingEnvironment environment)
            {
                extractedSink = environment.HerculesSink;
            }

            var vostokMultiHost = new VostokMultiHost(
                new VostokMultiHostSettings(
                    builder =>
                    {
                        builder.SetupApplicationIdentity(identityBuilder => identityBuilder.SetProject("testProject").SetEnvironment("testEnvironment"));
                        builder.SetupLog(logBuilder => logBuilder.SetupConsoleLog());
                    }));

            var extractIdentifier = ("testApplication", "testInstance");

            var extractApplication = new VostokMultiHostApplicationSettings(
                new TesterApplication(Extract),
                extractIdentifier,
                SetupMultiHostApplication);

            var assertIdentifier = ("testApplication", "testInstance2");

            var assertApplication = new VostokMultiHostApplicationSettings(
                new TesterApplication(Assert),
                assertIdentifier,
                SetupMultiHostApplication);

            await vostokMultiHost.StartAsync();

            await vostokMultiHost.RunSequentially(extractApplication, assertApplication);

            var results = await vostokMultiHost.StopAsync();

            // ReSharper disable once PossibleNullReferenceException
            results.ApplicationRunResults[assertIdentifier].EnsureSuccess();
        }

        [Test]
        public async Task Should_be_able_to_override_common_components()
        {
            IHerculesSink extractedSink = null;

            void Assert(IVostokHostingEnvironment environment)
            {
                environment.HerculesSink.Should().BeSameAs(extractedSink);
            }

            void Extract(IVostokHostingEnvironment environment)
            {
                extractedSink = environment.HerculesSink;
            }

            var initialSink = new DevNullHerculesSink();

            var vostokMultiHost = new VostokMultiHost(
                new VostokMultiHostSettings(
                    builder =>
                    {
                        builder.SetupApplicationIdentity(identityBuilder => identityBuilder.SetProject("testProject").SetEnvironment("testEnvironment"));
                        builder.SetupHerculesSink(sinkBuilder => sinkBuilder.UseInstance(initialSink));
                        builder.SetupLog(logBuilder => logBuilder.SetupConsoleLog());
                    }));

            var extractIdentifier = ("testApplication", "testInstance");

            var extractApplication = new VostokMultiHostApplicationSettings(
                new TesterApplication(Extract),
                extractIdentifier,
                SetupMultiHostApplication);

            var assertIdentifier = ("testApplication", "testInstance2");

            var assertApplication = new VostokMultiHostApplicationSettings(
                new TesterApplication(Assert),
                assertIdentifier,
                SetupMultiHostApplication);

            await vostokMultiHost.StartAsync();

            await vostokMultiHost.RunSequentially(extractApplication, assertApplication);

            var results = await vostokMultiHost.StopAsync();

            // ReSharper disable once PossibleNullReferenceException
            results.ApplicationRunResults[assertIdentifier].EnsureSuccess();

            extractedSink.Should().BeSameAs(initialSink);
        }
        
        private static void SetupMultiHostApplication(IVostokHostingEnvironmentBuilder builder)
        {
            builder.SetupLog(log => log.SetupConsoleLog());

            builder.SetupShutdownTimeout(1.Milliseconds());
        }

        private class TesterApplication : IVostokApplication
        {
            private Action<IVostokHostingEnvironment> act;

            public TesterApplication(Action<IVostokHostingEnvironment> act)
            {
                this.act = act;
            }

            public Task InitializeAsync(IVostokHostingEnvironment environment) => Task.CompletedTask;

            public Task RunAsync(IVostokHostingEnvironment environment)
            {
                act(environment);
                return Task.CompletedTask;
            }
        }

        private class TestHostExtension
        {
            public readonly int A;

            public TestHostExtension(int a)
            {
                A = a;
            }
        }
    }
}