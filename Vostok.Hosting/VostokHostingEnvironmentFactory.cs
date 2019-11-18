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
        /// <param name="setup">Delegate which configures <see cref="IVostokHostingEnvironment"/>.</param>
        /// <param name="shutdownToken">Cancellation token, that will be passed into <see cref="IVostokHostingEnvironment.ShutdownToken"/>.</param>
        [NotNull]
        public static IVostokHostingEnvironment Create([NotNull] VostokHostingEnvironmentSetup setup, CancellationToken shutdownToken = default)
            => EnvironmentBuilder.Build(setup ?? throw new ArgumentNullException(nameof(setup)), shutdownToken);
    }
}