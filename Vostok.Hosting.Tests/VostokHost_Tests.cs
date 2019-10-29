using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Models;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.Tests
{
    [TestFixture]
    internal class VostokHost_Tests
    {
        [Test]
        public void Should_run_simple_application()
        {
            var application = new SimpleApplication();

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

            var host = new VostokHost(new VostokHostSettings(application, environmentSetup));

            var result = host.RunAsync().GetAwaiter().GetResult();

            result.State.Should().Be(VostokApplicationState.Exited);

            application.Initialized.Should().BeTrue();
            application.Run.Should().BeTrue();
        }

        private class SimpleApplication : IVostokApplication
        {
            public bool Initialized;
            public bool Run;

            public Task InitializeAsync(IVostokHostingEnvironment environment)
            {
                Initialized = true;

                environment.Log.Info("Hello from app!");

                return Task.CompletedTask;
            }

            public Task RunAsync(IVostokHostingEnvironment environment)
            {
                Run = true;

                environment.Log.Info("Hello again from app!");
                
                return Task.CompletedTask;
            }
        }
    }
}