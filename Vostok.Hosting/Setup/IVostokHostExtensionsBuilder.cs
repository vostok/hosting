using JetBrains.Annotations;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokHostExtensionsBuilder
    {
        IVostokHostExtensionsBuilder Add<TExtension>(TExtension extension);

        IVostokHostExtensionsBuilder Add<TExtension>(TExtension extension, string key);
    }
}