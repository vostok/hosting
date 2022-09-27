using System;
using Vostok.Commons.Helpers;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hosting.Helpers;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Configuration;
using Vostok.Logging.Hercules;
using Vostok.Logging.Hercules.Configuration;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Log
{
    internal class HerculesLogBuilder : SwitchableComponent<IVostokHerculesLogBuilder>, IVostokHerculesLogBuilder, IBuilder<ILog>
    {
        private readonly LogRulesBuilder rulesBuilder;
        private readonly Customization<HerculesLogSettings> settingsCustomization;
        private readonly Customization<ILog> logCustomization;
        private volatile Func<string> apiKeyProvider;
        private volatile Func<LogLevel> minLevelProvider;
        private volatile string stream;

        public HerculesLogBuilder(LogRulesBuilder rulesBuilder)
        {
            this.rulesBuilder = rulesBuilder;
            settingsCustomization = new Customization<HerculesLogSettings>();
            logCustomization = new Customization<ILog>();
        }

        public IVostokHerculesLogBuilder SetStream(string stream)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            return this;
        }

        public IVostokHerculesLogBuilder SetApiKeyProvider(Func<string> apiKeyProvider)
        {
            this.apiKeyProvider = apiKeyProvider ?? throw new ArgumentNullException(nameof(apiKeyProvider));
            return this;
        }

        public IVostokHerculesLogBuilder SetupMinimumLevelProvider(Func<LogLevel> minLevelProvider)
        {
            this.minLevelProvider = minLevelProvider ?? throw new ArgumentNullException(nameof(minLevelProvider));
            return this;
        }

        public IVostokHerculesLogBuilder CustomizeLog(Func<ILog, ILog> logCustomization)
        {
            this.logCustomization.AddCustomization(logCustomization ?? throw new ArgumentNullException(nameof(logCustomization)));
            return this;
        }

        public IVostokHerculesLogBuilder AddRule(LogConfigurationRule rule)
        {
            rule = rule.WithLog(Logs.HerculesLogName);
            rulesBuilder.Add(rule);
            return this;
        }
        
        public IVostokHerculesLogBuilder CustomizeSettings(Action<HerculesLogSettings> settingsCustomization)
        {
            this.settingsCustomization.AddCustomization(settingsCustomization ?? throw new ArgumentNullException(nameof(settingsCustomization)));
            return this;
        }

        public ILog Build(BuildContext context)
        {
            if (!IsEnabled)
            {
                context.LogDisabled("HerculesLog");
                return null;
            }

            var herculesSink = context.HerculesSink;
            if (herculesSink == null)
            {
                context.LogDisabled("HerculesLog", "disabled HerculesSink");
                return null;
            }

            if (stream == null)
            {
                context.LogDisabled("HerculesLog", "unconfigured stream");
                return null;
            }

            if (apiKeyProvider != null)
                herculesSink.ConfigureStream(stream, new StreamSettings {ApiKeyProvider = apiKeyProvider});

            var settings = new HerculesLogSettings(herculesSink, stream);

            settingsCustomization.Customize(settings);

            ILog log = new HerculesLog(settings);

            if (minLevelProvider != null)
                log = log.WithMinimumLevel(minLevelProvider);

            return logCustomization.Customize(log);
        }
    }
}