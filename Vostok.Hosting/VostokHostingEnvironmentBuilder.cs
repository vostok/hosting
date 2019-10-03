using System.Threading;
using JetBrains.Annotations;
using Vostok.Hosting.Components.Environment;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting
{
    [PublicAPI]
    public static class VostokHostingEnvironmentBuilder
    {
        public static VostokHostingEnvironment Build([NotNull] VostokHostingEnvironmentSetup setup, CancellationToken shutdownToken = default)
        {
            var builder = new EnvironmentBuilder();
            setup(builder);
            return builder.Build(shutdownToken);
        }
    }
}