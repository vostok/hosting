using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Vostok.Commons.Helpers.Disposable;
using Vostok.Hosting.Abstractions.Diagnostics;

namespace Vostok.Hosting.Components.Diagnostics
{
    internal class DiagnosticInfo : IDiagnosticInfo
    {
        private readonly ConcurrentDictionary<DiagnosticEntry, Func<object>> providers;

        public DiagnosticInfo()
            => providers = new ConcurrentDictionary<DiagnosticEntry, Func<object>>();

        public IDisposable RegisterProvider(DiagnosticEntry entry, Func<object> provider)
        {
            if (!providers.TryAdd(entry, provider))
                throw new InvalidOperationException($"Provider with entry '{entry}' is already registered.");

            return new ActionDisposable(() => providers.TryRemove(entry, out _));
        }

        public IReadOnlyList<DiagnosticEntry> ListAll() 
            => providers.Select(pair => pair.Key).ToArray();

        public IReadOnlyDictionary<DiagnosticEntry, object> QueryAll()
            => providers.ToDictionary(pair => pair.Key, pair => QuerySafe(pair.Value));

        public bool TryQuery(DiagnosticEntry entry, out object info)
        {
            var foundProvider = providers.TryGetValue(entry, out var provider);

            info = foundProvider ? QuerySafe(provider) : null;

            return foundProvider;
        }

        private static object QuerySafe(Func<object> provider)
        {
            try
            {
                return provider();
            }
            catch (Exception error)
            {
                return $"ERROR: info provider failed with {error.GetType().Name}: '{error.Message}'";
            }
        }
    }
}
