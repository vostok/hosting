using JetBrains.Annotations;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public delegate void EnvironmentSetup<in T>([NotNull] T builder);
}