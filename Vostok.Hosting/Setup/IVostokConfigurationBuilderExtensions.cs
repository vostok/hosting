using System;
using JetBrains.Annotations;
using Vostok.ClusterConfig.Client.Abstractions;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Sources;
using Vostok.Hosting.Abstractions;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public static class IVostokConfigurationBuilderExtensions
    {
        /// <summary>
        /// <para>Sets up <see cref="IVostokHostingEnvironment.ConfigurationSource"/> for settings of type <see cref="TSettings"/> in <see cref="IVostokHostingEnvironment.ConfigurationProvider"/>.</para>
        /// <para>Optionally, <see cref="IVostokHostingEnvironment.ConfigurationSource"/> can also be scoped into given <paramref name="scope"/>.</para>
        /// </summary>
        public static IVostokConfigurationBuilder SetupSourceFor<TSettings>([NotNull] this IVostokConfigurationBuilder builder, [NotNull] params string[] scope) =>
            builder.CustomizeConfigurationContext(configurationContext =>
                configurationContext.ConfigurationProvider.SetupSourceFor<TSettings>(
                    configurationContext.ConfigurationSource.ScopeTo(scope)));
        
        /// <summary>
        /// <para>Sets up <see cref="IVostokHostingEnvironment.SecretConfigurationSource"/> for settings of type <see cref="TSettings"/> in <see cref="IVostokHostingEnvironment.SecretConfigurationProvider"/>.</para>
        /// <para>Optionally, <see cref="IVostokHostingEnvironment.SecretConfigurationSource"/> can also be scoped into given <paramref name="scope"/>.</para>
        /// </summary>
        public static IVostokConfigurationBuilder SetupSecretSourceFor<TSettings>([NotNull] this IVostokConfigurationBuilder builder, [NotNull] params string[] scope) =>
            builder.CustomizeConfigurationContext(configurationContext =>
                configurationContext.SecretConfigurationProvider.SetupSourceFor<TSettings>(
                    configurationContext.SecretConfigurationSource.ScopeTo(scope)));
        
        /// <summary>
        /// <para>Sets up <see cref="IVostokHostingEnvironment.ConfigurationSource"/> merged with <see cref="IVostokHostingEnvironment.SecretConfigurationSource"/> for settings of type <see cref="TSettings"/> in <see cref="IVostokHostingEnvironment.ConfigurationProvider"/>.</para>
        /// <para>Optionally, merged source can also be scoped into given <paramref name="scope"/>.</para>
        /// </summary>
        public static IVostokConfigurationBuilder SetupMergedSourceFor<TSettings>([NotNull] this IVostokConfigurationBuilder builder, [NotNull] params string[] scope) =>
            builder.CustomizeConfigurationContext(configurationContext =>
                configurationContext.ConfigurationProvider.SetupSourceFor<TSettings>(
                    configurationContext.MergedConfigurationSource.ScopeTo(scope)));

        public static IVostokConfigurationSourcesBuilder NestSources([NotNull] this IVostokConfigurationBuilder builder, [NotNull] params string[] scopes)
            => new NestedSourcesBuilder(builder, scopes);

        private class NestedSourcesBuilder : IVostokConfigurationSourcesBuilder
        {
            private readonly IVostokConfigurationBuilder builder;
            private readonly string[] scopes;

            public NestedSourcesBuilder(IVostokConfigurationBuilder builder, string[] scopes)
            {
                this.builder = builder;
                this.scopes = scopes;
            }

            public IVostokConfigurationSourcesBuilder AddSource(IConfigurationSource source)
            {
                builder.AddSource(source.Nest(scopes));
                return this;
            }

            public IVostokConfigurationSourcesBuilder AddSource(Func<IClusterConfigClient, IConfigurationSource> sourceProvider)
            {
                builder.AddSource(ccClient => sourceProvider(ccClient).Nest(scopes));
                return this;
            }

            public IVostokConfigurationSourcesBuilder AddSecretSource(IConfigurationSource source)
            {
                builder.AddSecretSource(source.Nest(scopes));
                return this;
            }
        }
    }
}