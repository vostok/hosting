using JetBrains.Annotations;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public delegate void EnvironmentSetup([NotNull] IEnvironmentBuilder builder);
}