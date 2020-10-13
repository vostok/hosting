using System.Threading.Tasks;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.VostokMultiHost
{
    public class VostokMultiHostSettings : VostokHostSettings
    {
        // ReSharper disable once AssignNullToNotNullAttribute
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