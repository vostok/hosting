﻿using System;
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
        private readonly Customization<HerculesLogSettings> settingsCustomization;
        private readonly Customization<ILog> logCustomization;
        private volatile Func<string> apiKeyProvider;
        private volatile string stream;
        private volatile bool enabled;

        public HerculesLogBuilder()
        {
            settingsCustomization = new Customization<HerculesLogSettings>();
            logCustomization = new Customization<ILog>();
        }

        public IVostokHerculesLogBuilder Enable()
        {
            enabled = true;
            return this;
        }

        public IVostokHerculesLogBuilder Disable()
        {
            enabled = false;
            return this;
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
            if (!enabled)
            {
                context.Log.LogDisabled("HerculesLog");
                return null;
            }

            var herculesSink = context.HerculesSink;
            if (herculesSink == null)
            {
                context.Log.LogDisabled("HerculesLog", "disabled HerculesSink");
                return null;
            }

            if (stream == null)
            {
                context.Log.LogDisabled("HerculesLog", "unconfigured stream");
                return null;
            }

            if (apiKeyProvider != null)
                herculesSink.ConfigureStream(stream, new StreamSettings {ApiKeyProvider = apiKeyProvider});

            var settings = new HerculesLogSettings(herculesSink, stream);

            settingsCustomization.Customize(settings);

            return logCustomization.Customize(new HerculesLog(settings));
        }
    }
}