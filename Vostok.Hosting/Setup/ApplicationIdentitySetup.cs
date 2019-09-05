using JetBrains.Annotations;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public delegate void ApplicationIdentitySetup([NotNull] IApplicationIdentityBuilder builder);
}