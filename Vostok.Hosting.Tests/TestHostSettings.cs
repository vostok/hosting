using JetBrains.Annotations;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.Tests
{
    internal class TestHostSettings : VostokHostSettings
    {
        public TestHostSettings([NotNull] IVostokApplication application, [NotNull] VostokHostingEnvironmentSetup environmentSetup)
            : base(application, environmentSetup)
        {
            WarmupConfiguration = false;
            WarmupZooKeeper = false;
        }
    }
}