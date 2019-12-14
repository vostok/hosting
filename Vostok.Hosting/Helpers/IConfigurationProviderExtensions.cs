using System;
using System.Reflection;
using Vostok.Configuration.Abstractions;

namespace Vostok.Hosting.Helpers
{
    internal static class IConfigurationProviderExtensions
    {
        private static readonly MethodInfo SetupMethod = typeof(IConfigurationProvider)
            .GetMethod(nameof(IConfigurationProvider.SetupSourceFor));

        private static readonly MethodInfo GetMethod = typeof(IConfigurationProvider)
            .GetMethod(nameof(IConfigurationProvider.Get), new Type[]{});

        public static void SetupSourceFor(this IConfigurationProvider provider, IConfigurationSource source, Type type)
            => SetupMethod.MakeGenericMethod(type).Invoke(provider, new object[] {source});

        public static object Get(this IConfigurationProvider provider, Type type)
            => GetMethod.MakeGenericMethod(type).Invoke(provider, Array.Empty<object>());
    }
}