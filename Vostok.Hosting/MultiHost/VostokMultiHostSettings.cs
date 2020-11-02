using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.MultiHost
{
    [PublicAPI]
    public class VostokMultiHostSettings : VostokHostSettings
    {
        public VostokMultiHostSettings(VostokHostingEnvironmentSetup builder)
            : base(new StubApplication(), builder)
        {
        }
    }

    internal class StubApplication : IVostokApplication
    {
        public Task InitializeAsync(IVostokHostingEnvironment environment) => Task.CompletedTask;

        public Task RunAsync(IVostokHostingEnvironment environment) => Task.CompletedTask;
    }
}