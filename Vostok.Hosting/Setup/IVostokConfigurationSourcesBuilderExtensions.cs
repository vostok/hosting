using System;
using JetBrains.Annotations;
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

        public static IVostokConfigurationSourcesBuilder AddSecretInMemoryObject([NotNull] this IVostokConfigurationSourcesBuilder builder, [NotNull] object @object)
            => builder.AddSecretSource(new ObjectSource(@object));

        public static IVostokConfigurationSourcesBuilder AddJsonFile([NotNull] this IVostokConfigurationSourcesBuilder builder, [NotNull] string path)
            => builder.AddSource(new JsonFileSource(path));

        public static IVostokConfigurationSourcesBuilder AddSecretJsonFile([NotNull] this IVostokConfigurationSourcesBuilder builder, [NotNull] string path)
            => builder.AddSecretSource(new JsonFileSource(path));

        public static IVostokConfigurationSourcesBuilder AddAppSettingsJson([NotNull] this IVostokConfigurationSourcesBuilder builder)
            => builder
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{GetDotnetEnvironment()}.json");

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

        private static string GetDotnetEnvironment()
            => Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ??
               Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
               "Production";
    }
}