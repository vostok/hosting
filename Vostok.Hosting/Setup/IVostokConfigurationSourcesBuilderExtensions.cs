using System;
using JetBrains.Annotations;
using Vostok.ClusterConfig.Client.Abstractions;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Sources;
using Vostok.Configuration.Sources.CommandLine;
using Vostok.Configuration.Sources.Environment;
using Vostok.Configuration.Sources.Json;
using Vostok.Configuration.Sources.Object;
using Vostok.Configuration.Sources.Xml;
using Vostok.Configuration.Sources.Yaml;
using Vostok.Hosting.Components.Configuration;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public static class IVostokConfigurationSourcesBuilderExtensions
    {
        public static IVostokConfigurationSourcesBuilder AddEnvironmentVariables([NotNull] this IVostokConfigurationSourcesBuilder builder)
            => builder.AddSource(new EnvironmentVariablesSource());

        public static IVostokConfigurationSourcesBuilder AddCommandLineArguments([NotNull] this IVostokConfigurationSourcesBuilder builder, [NotNull] string[] args)
            => builder.AddSource(new CommandLineSource(args));

        public static IVostokConfigurationSourcesBuilder AddCommandLineArguments([NotNull] this IVostokConfigurationSourcesBuilder builder, [NotNull] string[] args, [CanBeNull] string defaultKey, [CanBeNull] string defaultValue)
            => builder.AddSource(new CommandLineSource(args, defaultKey, defaultValue));

        public static IVostokConfigurationSourcesBuilder AddInMemoryObject([NotNull] this IVostokConfigurationSourcesBuilder builder, [NotNull] object @object)
            => builder.AddSource(new ObjectSource(@object));

        public static IVostokConfigurationSourcesBuilder AddJsonFile([NotNull] this IVostokConfigurationSourcesBuilder builder, [NotNull] string path)
            => builder.AddSource(new JsonFileSource(path));

        public static IVostokConfigurationSourcesBuilder AddSecretJsonFile([NotNull] this IVostokConfigurationSourcesBuilder builder, [NotNull] string path)
            => builder.AddSecretSource(new JsonFileSource(path));

        public static IVostokConfigurationSourcesBuilder AddAppSettingsJson([NotNull] this IVostokConfigurationSourcesBuilder builder)
            => builder
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json");

        public static IVostokConfigurationSourcesBuilder AddYamlFile([NotNull] this IVostokConfigurationSourcesBuilder builder, [NotNull] string path)
            => builder.AddSource(new YamlFileSource(path));

        public static IVostokConfigurationSourcesBuilder AddSecretYamlFile([NotNull] this IVostokConfigurationSourcesBuilder builder, [NotNull] string path)
            => builder.AddSecretSource(new YamlFileSource(path));

        public static IVostokConfigurationSourcesBuilder AddXmlFile([NotNull] this IVostokConfigurationSourcesBuilder builder, [NotNull] string path)
            => builder.AddSource(new XmlFileSource(path));

        public static IVostokConfigurationSourcesBuilder AddSecretXmlFile([NotNull] this IVostokConfigurationSourcesBuilder builder, [NotNull] string path)
            => builder.AddSecretSource(new XmlFileSource(path));

        public static IVostokConfigurationSourcesBuilder AddClusterConfig([NotNull] this IVostokConfigurationSourcesBuilder builder, [NotNull] string prefix)
            => builder.AddSource(ccClient => new ClusterConfigSourceWithParsers(ccClient, prefix));

        public static IVostokConfigurationSourcesBuilder NestSources([NotNull] this IVostokConfigurationSourcesBuilder builder, [NotNull] params string[] scopes)
            => new NestedSourcesBuilder(builder, scopes);

        private class NestedSourcesBuilder : IVostokConfigurationSourcesBuilder
        {
            private readonly IVostokConfigurationSourcesBuilder builder;
            private readonly string[] scopes;

            public NestedSourcesBuilder(IVostokConfigurationSourcesBuilder builder, string[] scopes)
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
