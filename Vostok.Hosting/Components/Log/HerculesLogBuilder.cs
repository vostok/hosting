using System;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hosting.Helpers;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Hercules;
using Vostok.Logging.Hercules.Configuration;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Log
{
    internal class HerculesLogBuilder : IVostokHerculesLogBuilder, IBuilder<ILog>
    {
        private Func<string> apiKeyProvider;
        private string stream;
        private readonly Customization<HerculesLogSettings> settingsCustomization;
        private readonly Customization<ILog> logCustomization;

        public HerculesLogBuilder()
        {
            settingsCustomization = new Customization<HerculesLogSettings>();
            logCustomization = new Customization<ILog>();
        }

        public IVostokHerculesLogBuilder SetStream(string stream)
        {
            this.stream = stream;
            return this;
        }

        public IVostokHerculesLogBuilder SetApiKeyProvider(Func<string> apiKeyProvider)
        {
            this.apiKeyProvider = apiKeyProvider;
            return this;
        }

        public IVostokHerculesLogBuilder CustomizeLog(Func<ILog, ILog> logCustomization)
        {
            this.logCustomization.AddCustomization(logCustomization);
            return this;
        }

        public IVostokHerculesLogBuilder CustomizeSettings(Action<HerculesLogSettings> settingsCustomization)
        {
            this.settingsCustomization.AddCustomization(settingsCustomization);
            return this;
        }

        public ILog Build(BuildContext context)
        {
            var herculesSink = context.HerculesSink;
            
            // CR(iloktionov): Let's log something about every component that's not working as intended due to missing configuration.
            if (herculesSink == null || stream == null)
                return null;

            if (apiKeyProvider != null)
                herculesSink.ConfigureStream(stream, new StreamSettings {ApiKeyProvider = apiKeyProvider});

            var settings = new HerculesLogSettings(herculesSink, stream);

            settingsCustomization.Customize(settings);

            var log = new HerculesLog(settings);

            logCustomization.Customize(log);

            return log;
        }
    }
}