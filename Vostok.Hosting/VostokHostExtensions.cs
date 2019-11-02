using JetBrains.Annotations;
using Vostok.Hosting.Models;

namespace Vostok.Hosting
{
    /// <summary>
    /// A set of extensions for <see cref="VostokHost"/>.
    /// </summary>
    [PublicAPI]
    public static class VostokHostExtensions
    {
        /// <inheritdoc cref="VostokHost.RunAsync"/>
        public static VostokApplicationRunResult Run([NotNull] this VostokHost vostokHost) =>
            vostokHost.RunAsync().GetAwaiter().GetResult();
    }
}