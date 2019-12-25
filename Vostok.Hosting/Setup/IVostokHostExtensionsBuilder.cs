using JetBrains.Annotations;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokHostExtensionsBuilder
    {
        IVostokHostExtensionsBuilder Add<TExtension>([NotNull] TExtension extension);
        IVostokHostExtensionsBuilder Add<TExtension>([NotNull] string key, [NotNull] TExtension extension);

        IVostokHostExtensionsBuilder AddDisposable<TExtension>([NotNull] TExtension extension);
        IVostokHostExtensionsBuilder AddDisposable<TExtension>([NotNull] string key, [NotNull] TExtension extension);
    }
}