using JetBrains.Annotations;
using Vostok.Hosting.Abstractions;

namespace Vostok.Hosting.Setup
{
    /// <summary>
    /// Delegate which configures <see cref="IVostokHostingEnvironment"/>.
    /// </summary>
    [PublicAPI]
    public delegate void VostokHostingEnvironmentSetup([NotNull] IVostokHostingEnvironmentBuilder builder);
}