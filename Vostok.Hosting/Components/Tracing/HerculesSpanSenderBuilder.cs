using System;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hosting.Components.Hercules;
using Vostok.Hosting.Setup;
using Vostok.Tracing.Abstractions;
using Vostok.Tracing.Hercules;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Tracing
{
    internal class HerculesSpanSenderBuilder : IHerculesSpanSenderBuilder, IBuilder<ISpanSender>
    {
        private IBuilder<Func<string>> apiKeyProviderBuilder;
        private string stream;

        public IHerculesSpanSenderBuilder SetStream(string stream)
        {
            this.stream = stream;
            return this;
        }

        public IHerculesSpanSenderBuilder SetApiKeyProvider(Func<string> apiKeyProvider)
        {
            apiKeyProviderBuilder = new CustomApiKeyProviderBuilder(apiKeyProvider);
            return this;
        }

        public IHerculesSpanSenderBuilder SetClusterConfigApiKeyProvider(string path)
        {
            apiKeyProviderBuilder = new ClusterConfigApiKeyProvider(path);
            return this;
        }
        
        public ISpanSender Build(BuildContext context)
        {
            var herculesSink = context.HerculesSink;

            if (herculesSink == null || stream == null)
                return new DevNullSpanSender();

            var apiKeyProvider = apiKeyProviderBuilder?.Build(context);
            if (apiKeyProvider != null)
                herculesSink.ConfigureStream(stream, new StreamSettings { ApiKeyProvider = apiKeyProvider });

            var settings = new HerculesSpanSenderSettings(herculesSink, stream);

            return new HerculesSpanSender(settings);
        }
    }
}