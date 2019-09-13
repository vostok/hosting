using System;

namespace Vostok.Hosting.Components.Hercules
{
    internal class ClusterConfigApiKeyProvider : IBuilder<Func<string>>
    {
        private readonly string path;

        public ClusterConfigApiKeyProvider(string path)
        {
            this.path = path;
        }

        public Func<string> Build(Context context)
        {
            return context.ClusterConfigClient == null
                ? (Func<string>)null
                : () => context.ClusterConfigClient.Get(path)?.Value;
        }
    }
}