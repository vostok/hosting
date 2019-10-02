using System;
using JetBrains.Annotations;

namespace Vostok.Hosting.Components.String
{
    internal class StringProviderBuilder : IBuilder<Func<string>>
    {
        private readonly Func<BuildContext, Func<string>> provider;

        private StringProviderBuilder(Func<BuildContext, Func<string>> provider)
        {
            this.provider = provider;
        }

        public static StringProviderBuilder FromValue(string value)
        {
            return new StringProviderBuilder(context => () => value);
        }

        public static StringProviderBuilder FromValueProvider(Func<string> valueProvider) =>
            new StringProviderBuilder(context => valueProvider);

        public static StringProviderBuilder FromClusterConfig(string path, string defaultValue = null) =>
            new StringProviderBuilder(
                context =>
                    context.ClusterConfigClient == null
                        ? (Func<string>)(() => defaultValue)
                        : () => context.ClusterConfigClient.Get(path)?.Value);

        [CanBeNull]
        public Func<string> Build(BuildContext context) => provider(context);
    }
}