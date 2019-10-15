using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.Components.HostExtensions
{
    internal class HostExtensionsBuilder : IVostokHostExtensionsBuilder, IBuilder<IVostokHostExtensions>
    {
        private HostExtensions extensions;

        public HostExtensionsBuilder()
        {
            extensions = new HostExtensions();
        }


        public IVostokHostExtensions Build(BuildContext context)
        {
            if (context.ZooKeeperClient != null)
                extensions.Add(context.ZooKeeperClient);

            return extensions;
        }

        public IVostokHostExtensionsBuilder Add<TExtension>(TExtension extension)
        {
            extensions.Add(extension);
            return this;
        }

        public IVostokHostExtensionsBuilder Add<TExtension>(TExtension extension, string key)
        {
            extensions.Add(extension);
            return this;
        }
    }
}