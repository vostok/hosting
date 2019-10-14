using System.Threading;
using JetBrains.Annotations;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Components.Environment;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting
{
    [PublicAPI]
    public static class VostokHostingEnvironmentFactory
    {
        public static IVostokHostingEnvironment Create([NotNull] VostokHostingEnvironmentSetup setup, CancellationToken shutdownToken = default)
        {
            return EnvironmentBuilder.Build(setup, shutdownToken);
        }
    }
}