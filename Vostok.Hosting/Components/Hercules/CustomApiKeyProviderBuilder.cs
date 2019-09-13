using System;

namespace Vostok.Hosting.Components.Hercules
{
    internal class CustomApiKeyProviderBuilder : IBuilder<Func<string>>
    {
        private readonly Func<string> apiKeyProvider;

        public CustomApiKeyProviderBuilder(Func<string> apiKeyProvider)
        {
            this.apiKeyProvider = apiKeyProvider;
        }

        public Func<string> Build(Context context) => apiKeyProvider;
    }
}