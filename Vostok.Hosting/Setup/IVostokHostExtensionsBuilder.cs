using System;
using JetBrains.Annotations;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokHostExtensionsBuilder
    {
        IVostokHostExtensionsBuilder Add([NotNull] Type type, [NotNull] object extension);
        IVostokHostExtensionsBuilder Add([NotNull] Type type, [NotNull] string key, [NotNull] object extension);

        IVostokHostExtensionsBuilder Add<TExtension>([NotNull] TExtension extension);
        IVostokHostExtensionsBuilder Add<TExtension>([NotNull] string key, [NotNull] TExtension extension);

        IVostokHostExtensionsBuilder AddDisposable([NotNull] Type type, [NotNull] object extension);
        IVostokHostExtensionsBuilder AddDisposable([NotNull] Type type, [NotNull] string key, [NotNull] object extension);

        IVostokHostExtensionsBuilder AddDisposable<TExtension>([NotNull] TExtension extension);
        IVostokHostExtensionsBuilder AddDisposable<TExtension>([NotNull] string key, [NotNull] TExtension extension);
    }
}