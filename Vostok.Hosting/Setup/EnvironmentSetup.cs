using JetBrains.Annotations;

namespace Vostok.Hosting.Setup
{
    // CR(iloktionov): System.Action<T>?
    [PublicAPI]
    public delegate void EnvironmentSetup<in T>([NotNull] T builder);
}