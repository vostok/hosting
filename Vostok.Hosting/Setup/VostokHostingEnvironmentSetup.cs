using JetBrains.Annotations;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public delegate void VostokHostingEnvironmentSetup([NotNull] IVostokHostingEnvironmentBuilder builder);
}