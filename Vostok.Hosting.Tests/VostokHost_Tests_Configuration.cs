using System;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Configuration.Abstractions.Merging;
using Vostok.Configuration.Extensions;
using Vostok.Configuration.Sources;
using Vostok.Configuration.Sources.Object;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Requirements;
using Vostok.Hosting.Models;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;

// ReSharper disable UnassignedGetOnlyAutoProperty

namespace Vostok.Hosting.Tests
{
    [TestFixture]
    internal class VostokHost_Tests_Configuration
    {
        private IVostokApplication application;
        private VostokHost host;

        [Test]
        public async Task Should_allow_to_use_statically_configured_app_identity_properties_during_configuration_setup()
        {
            application = new Application();

            host = new VostokHost(new TestHostSettings(application,
                builder =>
                {
                    builder.SetupApplicationIdentity(
                        id => id
                            .SetProject("infra")
                            .SetSubproject("vostok")
                            .SetApplication("app")
                            .SetInstance("1"));

                    builder.SetupApplicationIdentity((id, ctx) => id.SetEnvironment("env"));

                    builder.SetupLog(log => log.SetupConsoleLog());

                    builder.SetupConfiguration(
                        (config, ctx) =>
                        {
                            ctx.Log.Info(ctx.ApplicationIdentity.ToString());

                            ctx.ApplicationIdentity.Project.Should().Be("infra");
                            ctx.ApplicationIdentity.Subproject.Should().Be("vostok");
                            ctx.ApplicationIdentity.Application.Should().Be("app");
                            ctx.ApplicationIdentity.Instance.Should().Be("1");

                            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                            Action action = () => ctx.ApplicationIdentity.Environment.GetHashCode();

                            var exception = action.Should().Throw<InvalidOperationException>().Which;

                            Console.Out.WriteLine(exception);
                        });
                }));

            var result = await host.RunAsync();

            result.State.Should().Be(VostokApplicationState.Exited);
        }

        [Test]
        public async Task Should_allow_to_setup_sources_in_an_idempotent_way()
        {
            application = new Application(
                env =>
                {
                    env.ConfigurationProvider.SetupSourceFor<ApplicationSettings>(env.ConfigurationSource);
                    env.SecretConfigurationProvider.SetupSourceFor<ApplicationSecretSettings>(env.SecretConfigurationSource);
                });

            host = new VostokHost(new TestHostSettings(application, SetupEnvironment));

            var result = await host.RunAsync();

            result.State.Should().Be(VostokApplicationState.Exited);
        }

        [Test]
        public async Task Should_support_well_known_substitutions()
        {
            application = new Application(
                env =>
                {
                    Action assertion = () =>
                    {
                        var settings = env.ConfigurationProvider.Get<ApplicationSettings>();
                        var secrets = env.SecretConfigurationProvider.Get<ApplicationSecretSettings>();

                        settings.A.Should().Be("infra");
                        settings.B.Should().Be("vostok");
                        settings.C.Should().Be("app");
                        settings.D.Should().Be("dev");
                        settings.E.Should().Be("1");

                        secrets.F.Should().Be("sd-app");
                        secrets.G.Should().Be("sd-env");
                    };

                    assertion.ShouldPassIn(10.Seconds(), 100.Milliseconds());
                });

            host = new VostokHost(new TestHostSettings(application,
                builder =>
                {
                    builder.SetupApplicationIdentity(
                        id => id
                            .SetProject("infra")
                            .SetSubproject("vostok")
                            .SetEnvironment("dev")
                            .SetApplication("app")
                            .SetInstance("1"));

                    builder.SetupLog(log => log.SetupConsoleLog());

                    builder.SetBeaconApplication("sd-app");
                    builder.SetBeaconEnvironment("sd-env");

                    builder.SetupConfiguration(
                        config =>
                        {
                            config.AddSource(new ObjectSource(new
                            {
                                A = $"#{{{VostokConfigurationPlaceholders.IdentityProject}}}",
                                B = $"#{{{VostokConfigurationPlaceholders.IdentitySubproject}}}",
                                C = $"#{{{VostokConfigurationPlaceholders.IdentityApplication}}}",
                                D = $"#{{{VostokConfigurationPlaceholders.IdentityEnvironment}}}",
                                E = $"#{{{VostokConfigurationPlaceholders.IdentityInstance}}}"
                            }));

                            config.AddSecretSource(new ObjectSource(new
                            {
                                F = $"#{{{VostokConfigurationPlaceholders.ServiceDiscoveryApplication}}}",
                                G = $"#{{{VostokConfigurationPlaceholders.ServiceDiscoveryEnvironment}}}",
                            }));
                        });

                    builder.SetupHerculesSink(
                        (sink, context) =>
                        {
                            context.ConfigurationProvider.Get<ApplicationSettings>();
                            context.SecretConfigurationProvider.Get<ApplicationSecretSettings>();
                        });
                }));

            var result = await host.RunAsync();

            result.State.Should().Be(VostokApplicationState.Exited);
        }

        [TestCase(LogLevel.Debug)]
        [TestCase(LogLevel.Warn)]
        public async Task Should_allow_to_use_configuration_during_logging_setup(LogLevel logLevel)
        {
            application = new Application();

            host = new VostokHost(new TestHostSettings(application,
                builder =>
                {
                    builder.SetupApplicationIdentity(
                        id => id
                            .SetProject("infra")
                            .SetSubproject("vostok")
                            .SetApplication("app")
                            .SetInstance("1"));

                    builder.SetupApplicationIdentity((id, ctx) => id.SetEnvironment("env"));

                    builder.SetupLog((b, ctx) =>
                    {
                        b.SetupConsoleLog(
                            c => c.SetupMinimumLevelProvider(
                                () => ctx.ConfigurationProvider.Get<ApplicationSettings>().LogLevel));
                    });

                    builder.SetupConfiguration(
                        config =>
                        {
                            config.AddSource(new ObjectSource(new
                            {
                                LogLevel = logLevel
                            }));
                        });
                }));

            var result = await host.RunAsync();

            result.State.Should().Be(VostokApplicationState.Exited);
        }
        
        [Test]
        public async Task Should_allow_to_use_configuration_during_configuration_setup()
        {
            application = new Application();

            host = new VostokHost(new TestHostSettings(application,
                builder =>
                {
                    builder.SetupApplicationIdentity(
                        id => id
                            .SetProject("infra")
                            .SetSubproject("vostok")
                            .SetApplication("app")
                            .SetInstance("1"));

                    builder.SetupApplicationIdentity((id, ctx) => id.SetEnvironment("env"));

                    builder.SetupConfiguration(
                        config =>
                        {
                            config.AddSource(new ObjectSource(new
                            {
                                A = "hello"
                            }));
                        });

                    builder.SetupConfiguration(
                        config =>
                            config.GetIntermediateConfiguration<ApplicationSettings>().A.Should().Be("hello"));
                }));

            var result = await host.RunAsync();

            result.State.Should().Be(VostokApplicationState.Exited);
        }

        [Test]
        public async Task Should_allow_to_use_datacenters_during_clusterconfig_setup()
        {
            application = new Application();

            host = new VostokHost(new TestHostSettings(application,
                builder =>
                {
                    builder.SetupApplicationIdentity(
                        id => id
                            .SetProject("infra")
                            .SetSubproject("vostok")
                            .SetApplication("app")
                            .SetInstance("1"));

                    builder.SetupApplicationIdentity((id, ctx) => id.SetEnvironment("env"));

                    builder.SetupLog(b => b.SetupConsoleLog());

                    builder.SetupConfiguration(
                        (config, context) => { context.Datacenters.GetLocalDatacenter().Should().BeNull(); });
                }));

            var result = await host.RunAsync();

            result.State.Should().Be(VostokApplicationState.Exited);
        }

        [Test]
        public async Task Should_combine_sources_in_order()
        {
            application = new Application(
                env =>
                {
                    env.Log.Info("Configuration: {Configuration}", env.ConfigurationSource.Get());
                    var array = env.ConfigurationProvider.Get<int[]>(env.ConfigurationSource.ScopeTo("Array"));
                    array.Should().BeEquivalentTo(new[] {1, 2, 3, 4}, options => options.WithStrictOrdering());
                });

            host = new VostokHost(new TestHostSettings(application, setup =>
            {
                SetupEnvironment(setup);

                setup.SetupConfiguration(c => c.AddSource(new ObjectSource(new {Array = new[] {1}})));
                setup.SetupConfiguration(c => c.AddClusterConfig("settings2.json"));
                setup.SetupConfiguration(c => c.AddSource(new ObjectSource(new {Array = new[] {3}})));
                setup.SetupConfiguration(c => c.AddClusterConfig("settings4.json"));

                setup.SetupConfiguration(c => c.CustomizeSettingsMerging(s => s.ArrayMergeStyle = ArrayMergeStyle.Concat));
            }));

            var result = await host.RunAsync();

            result.State.Should().Be(VostokApplicationState.Exited);
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

            builder.SetupConfiguration(
                config =>
                {
                    config.AddSource(new ObjectSource(new {}));
                    config.AddSecretSource(new ObjectSource(new {}));
                });
        }

        [RequiresConfiguration(typeof(ApplicationSettings))]
        [RequiresSecretConfiguration(typeof(ApplicationSecretSettings))]
        private class Application : IVostokApplication
        {
            private readonly Action<IVostokHostingEnvironment> payload;

            public Application(Action<IVostokHostingEnvironment> payload = null)
                => this.payload = payload;

            public Task InitializeAsync(IVostokHostingEnvironment environment)
                => Task.CompletedTask;

            public Task RunAsync(IVostokHostingEnvironment environment)
            {
                payload?.Invoke(environment);

                return Task.CompletedTask;
            }
        }

        private class ApplicationSettings
        {
            public string A { get; }
            public string B { get; }
            public string C { get; }
            public string D { get; }
            public string E { get; }
            public LogLevel LogLevel { get; } = LogLevel.Debug;
        }

        private class ApplicationSecretSettings
        {
            public string F { get; }
            public string G { get; }
        }
    }
}