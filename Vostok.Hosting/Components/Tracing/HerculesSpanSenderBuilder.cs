using System;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hosting.Components.String;
using Vostok.Hosting.Setup;
using Vostok.Tracing.Abstractions;
using Vostok.Tracing.Hercules;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Tracing
{
    internal class HerculesSpanSenderBuilder : IHerculesSpanSenderBuilder, IBuilder<ISpanSender>
    {
        private StringProviderBuilder apiKeyProviderBuilder;
        private StringProviderBuilder streamProviderBuilder;

        public IHerculesSpanSenderBuilder SetStream(string stream)
        {
            streamProviderBuilder = StringProviderBuilder.FromValue(stream);
            return this;
        }

        public IHerculesSpanSenderBuilder SetStreamFromClusterConfig(string path)
        {
            streamProviderBuilder = StringProviderBuilder.FromClusterConfig(path);
            return this;
        }

        public IHerculesSpanSenderBuilder SetApiKeyProvider(Func<string> apiKeyProvider)
        {
            apiKeyProviderBuilder = StringProviderBuilder.FromValueProvider(apiKeyProvider);
            return this;
        }

        public IHerculesSpanSenderBuilder SetClusterConfigApiKeyProvider(string path)
        {
            apiKeyProviderBuilder = StringProviderBuilder.FromClusterConfig(path);
            return this;
        }
        
        public ISpanSender Build(BuildContext context)
        {
            var herculesSink = context.HerculesSink;
            var stream = streamProviderBuilder?.Build(context)?.Invoke();

            if (herculesSink == null || stream == null)
                return null;

            var apiKeyProvider = apiKeyProviderBuilder?.Build(context);
            if (apiKeyProvider != null)
                herculesSink.ConfigureStream(stream, new StreamSettings { ApiKeyProvider = apiKeyProvider });

            var settings = new HerculesSpanSenderSettings(herculesSink, stream);

            return new HerculesSpanSender(settings);
        }
    }
}