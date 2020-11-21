using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.MultiHost
{
    // CR(iloktionov): Remove inheritance.
    [PublicAPI]
    public class VostokMultiHostSettings : VostokHostSettings
    {
        public VostokMultiHostSettings(VostokHostingEnvironmentSetup builder)
            : base(new StubApplication(), builder)
        {
        }

        private class StubApplication : IVostokApplication
        {
            public Task InitializeAsync(IVostokHostingEnvironment environment) => Task.CompletedTask;

            public Task RunAsync(IVostokHostingEnvironment environment) => Task.CompletedTask;
        }
    }
}