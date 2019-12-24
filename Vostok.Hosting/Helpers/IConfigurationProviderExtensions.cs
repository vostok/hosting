using System;
using System.Reflection;
using Vostok.Configuration.Abstractions;

namespace Vostok.Hosting.Helpers
{
    internal static class IConfigurationProviderExtensions
    {
        private static readonly MethodInfo SetupMethod = typeof(IConfigurationProvider)
            .GetMethod(nameof(IConfigurationProvider.SetupSourceFor));

        public static void SetupSourceFor(this IConfigurationProvider provider, IConfigurationSource source, Type type)
            => SetupMethod.MakeGenericMethod(type).Invoke(provider, new object[] {source});
    }
}