using System;
using JetBrains.Annotations;
using Vostok.ClusterConfig.Client.Abstractions;
using Vostok.Configuration;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.Merging;
using Vostok.Configuration.Printing;
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

        public static IVostokConfigurationBuilder AddXmlFile([NotNull] this IVostokConfigurationBuilder builder, [NotNull] string path)
            => builder.AddSource(new XmlFileSource(path));

        public static IVostokConfigurationBuilder AddSecretXmlFile([NotNull] this IVostokConfigurationBuilder builder, [NotNull] string path)
            => builder.AddSecretSource(new XmlFileSource(path));

        public static IVostokConfigurationBuilder AddClusterConfig([NotNull] this IVostokConfigurationBuilder builder, [NotNull] string prefix)
            => builder.AddSource(ccClient => new ClusterConfigSourceWithParsers(ccClient, prefix));

        public static IVostokConfigurationBuilder NestSources([NotNull] this IVostokConfigurationBuilder builder, [NotNull] params string[] scopes)
            => new NestedSourcesBuilder(builder, scopes);

        private class NestedSourcesBuilder : IVostokConfigurationBuilder
        {
            private readonly IVostokConfigurationBuilder builder;
            private readonly string[] scopes;

            public NestedSourcesBuilder(IVostokConfigurationBuilder builder, string[] scopes)
            {
                this.builder = builder;
                this.scopes = scopes;
            }

            public IVostokConfigurationBuilder AddSource(IConfigurationSource source)
            {
                builder.AddSource(source.Nest(scopes));
                return this;
            }

            public IVostokConfigurationBuilder AddSource(Func<IClusterConfigClient, IConfigurationSource> sourceProvider)
            {
                builder.AddSource(ccClient => sourceProvider(ccClient).Nest(scopes));
                return this;
            }

            public IVostokConfigurationBuilder AddSecretSource(IConfigurationSource source)
            {
                builder.AddSecretSource(source.Nest(scopes));
                return this;
            }

            public IVostokConfigurationBuilder CustomizeConfigurationContext(Action<IVostokConfigurationContext> configurationContextCustomization)
            {
                builder.CustomizeConfigurationContext(configurationContextCustomization);
                return this;
            }

            public IVostokConfigurationBuilder CustomizeSettingsMerging(Action<SettingsMergeOptions> settingsCustomization)
            {
                builder.CustomizeSettingsMerging(settingsCustomization);
                return this;
            }

            public IVostokConfigurationBuilder CustomizeSecretSettingsMerging(Action<SettingsMergeOptions> settingsCustomization)
            {
                builder.CustomizeSecretSettingsMerging(settingsCustomization);
                return this;
            }

            public IVostokConfigurationBuilder CustomizeConfigurationProvider(Action<ConfigurationProviderSettings> settingsCustomization)
            {
                builder.CustomizeConfigurationProvider(settingsCustomization);
                return this;
            }

            public IVostokConfigurationBuilder CustomizeSecretConfigurationProvider(Action<ConfigurationProviderSettings> settingsCustomization)
            {
                builder.CustomizeSecretConfigurationProvider(settingsCustomization);
                return this;
            }

            public IVostokConfigurationBuilder CustomizePrintSettings(Action<PrintSettings> settingsCustomization)
            {
                builder.CustomizePrintSettings(settingsCustomization);
                return this;
            }
        }
    }
}
