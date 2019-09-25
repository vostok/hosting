using System;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.Components.Hercules
{
    internal class ClusterConfigApiKeyProvider : IBuilder<Func<string>>
    {
        private readonly string path;

        public ClusterConfigApiKeyProvider(string path)
        {
            this.path = path;
        }

        public Func<string> Build(BuildContext context)
        {
            return context.ClusterConfigClient == null
                ? (Func<string>)null
                : () => context.ClusterConfigClient.Get(path)?.Value;
        }
    }
}