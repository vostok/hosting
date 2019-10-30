using System;
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
        /// <summary>
        /// Creates an instance of <see cref="IVostokHostingEnvironment"/>, using given <paramref name="setup"/> and <paramref name="shutdownToken"/>.
        /// </summary>
        [NotNull]
        public static IVostokHostingEnvironment Create([NotNull] VostokHostingEnvironmentSetup setup, CancellationToken shutdownToken = default)
        {
            return EnvironmentBuilder.Build(
                setup ?? throw new ArgumentNullException(nameof(setup)),
                shutdownToken);
        }
    }
}