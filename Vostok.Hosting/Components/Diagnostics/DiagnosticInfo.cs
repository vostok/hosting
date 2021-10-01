using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Vostok.Commons.Helpers.Disposable;
using Vostok.Hosting.Abstractions.Diagnostics;

namespace Vostok.Hosting.Components.Diagnostics
{
    internal class DiagnosticInfo : IDiagnosticInfo, IDisposable
    {
        private readonly ConcurrentDictionary<DiagnosticEntry, IDiagnosticInfoProvider> providers;

        public DiagnosticInfo()
            => providers = new ConcurrentDictionary<DiagnosticEntry, IDiagnosticInfoProvider>();

        public IDisposable RegisterProvider(DiagnosticEntry entry, IDiagnosticInfoProvider provider)
        {
            if (!providers.TryAdd(entry, provider))
                throw new InvalidOperationException($"Provider with entry '{entry}' is already registered.");

            return new ActionDisposable(() =>
            {
                if (providers.TryRemove(entry, out _))
                    (provider as IDisposable)?.Dispose();
            });
        }

        public IReadOnlyList<DiagnosticEntry> ListAll() 
            => providers.Select(pair => pair.Key).ToArray();

        public bool TryQuery(DiagnosticEntry entry, out object info)
        {
            var foundProvider = providers.TryGetValue(entry, out var provider);

            info = foundProvider ? provider.QuerySafe() : null;

            return foundProvider;
        }

        public void Dispose()
        {
            foreach (var provider in providers.Values.OfType<IDisposable>())
                provider.Dispose();
        }
    }
}
