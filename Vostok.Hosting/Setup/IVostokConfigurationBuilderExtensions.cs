using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Configuration.Sources.ClusterConfig;
using Vostok.Configuration.Sources.CommandLine;
using Vostok.Configuration.Sources.Environment;
using Vostok.Configuration.Sources.Json;
using Vostok.Configuration.Sources.Object;
using Vostok.Configuration.Sources.Xml;
using Vostok.Configuration.Sources.Yaml;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public static class IVostokConfigurationBuilderExtensions
    {
        public static IVostokConfigurationBuilder AddEnvironmentVariables([NotNull] this IVostokConfigurationBuilder builder)
            => builder.AddSource(new EnvironmentVariablesSource());

        public static IVostokConfigurationBuilder AddCommandLineArguments([NotNull] this IVostokConfigurationBuilder builder, [NotNull] string[] args)
            => builder.AddSource(new CommandLineSource(args));

        public static IVostokConfigurationBuilder AddCommandLineArguments([NotNull] this IVostokConfigurationBuilder builder, [NotNull] string[] args, [CanBeNull] string defaultKey, [CanBeNull] string defaultValue)
            => builder.AddSource(new CommandLineSource(args, defaultKey, defaultValue));

        public static IVostokConfigurationBuilder AddInMemoryObject([NotNull] this IVostokConfigurationBuilder builder, [NotNull] object @object)
            => builder.AddSource(new ObjectSource(@object));

        public static IVostokConfigurationBuilder AddJsonFile([NotNull] this IVostokConfigurationBuilder builder, [NotNull] string path)
            => builder.AddSource(new JsonFileSource(path));

        public static IVostokConfigurationBuilder AddSecretJsonFile([NotNull] this IVostokConfigurationBuilder builder, [NotNull] string path)
            => builder.AddSecretSource(new JsonFileSource(path));

        public static IVostokConfigurationBuilder AddAppSettingsJson([NotNull] this IVostokConfigurationBuilder builder)
            => builder
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json");

        public static IVostokConfigurationBuilder AddYamlFile([NotNull] this IVostokConfigurationBuilder builder, [NotNull] string path)
            => builder.AddSource(new YamlFileSource(path));

        public static IVostokConfigurationBuilder AddSecretYamlFile([NotNull] this IVostokConfigurationBuilder builder, [NotNull] string path)
            => builder.AddSecretSource(new YamlFileSource(path));

        public static IVostokConfigurationBuilder AddClusterConfig([NotNull] this IVostokConfigurationBuilder builder, [NotNull] string prefix)
            => builder.AddSource(ccClient => new ClusterConfigSource(new ClusterConfigSourceSettings(ccClient, prefix)
            {
                ConditionalValuesParsers = new List<(ValueNodeParser, ValueNodeCondition)>
                {
                    (XmlConfigurationParser.Parse, node => node.Name != null && node.Name.EndsWith(".xml")),
                    (JsonConfigurationParser.Parse, node => node.Name != null && node.Name.EndsWith(".json")),
                    (YamlConfigurationParser.Parse, node => node.Name != null && (node.Name.EndsWith(".yaml") || node.Name.EndsWith("yml")))
                }
            }));
    }
}
