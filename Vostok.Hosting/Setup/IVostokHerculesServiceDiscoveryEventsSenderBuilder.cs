using System;
using JetBrains.Annotations;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokHerculesServiceDiscoveryEventsSenderBuilder
    {
        bool IsEnabled { get; }

        IVostokHerculesServiceDiscoveryEventsSenderBuilder Enable();
        IVostokHerculesServiceDiscoveryEventsSenderBuilder Disable();

        IVostokHerculesServiceDiscoveryEventsSenderBuilder SetStream([NotNull] string stream);
        IVostokHerculesServiceDiscoveryEventsSenderBuilder SetApiKeyProvider([NotNull] Func<string> apiKeyProvider);
    }
}