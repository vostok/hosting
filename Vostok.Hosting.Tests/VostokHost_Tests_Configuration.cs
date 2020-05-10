﻿using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
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
                    var settings = env.ConfigurationProvider.Get<ApplicationSettings>();
                    var secrets = env.SecretConfigurationProvider.Get<ApplicationSecretSettings>();

                    settings.A.Should().Be("infra");
                    settings.B.Should().Be("subinfra");
                    settings.C.Should().Be("vostok");
                    settings.D.Should().Be("app");
                    settings.E.Should().Be("1");

                    secrets.F.Should().Be("sd-app");
                    secrets.G.Should().Be("sd-env");
                });

            host = new VostokHost(new TestHostSettings(application,
                builder =>
                {
                    builder.SetupApplicationIdentity(
                        id => id
                            .SetProject("infra")
                            .SetSubproject("subinfra")
                            .SetSubproject("vostok")
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
                }));

            var result = await host.RunAsync();

            result.State.Should().Be(VostokApplicationState.Exited);
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
        }

        private class ApplicationSecretSettings
        {
            public string F { get; }
            public string G { get; }
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
    }
}
