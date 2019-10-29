using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Vostok.Hosting.Abstractions;

namespace Vostok.Hosting.Components.HostExtensions
{
    internal class HostExtensions : IVostokHostExtensions
    {
        private readonly ConcurrentDictionary<Type, object> byType = new ConcurrentDictionary<Type, object>();
        private readonly ConcurrentDictionary<(string, Type), object> byKey = new ConcurrentDictionary<(string, Type), object>();

        public TExtension Get<TExtension>()
        {
            if (!byType.TryGetValue(typeof(TExtension), out var obj))
                throw new KeyNotFoundException($"Host extension with type '{typeof(TExtension)}' has not been registered.");

            return (TExtension)obj;
        }

        public TExtension Get<TExtension>(string key)
        {
            if (!byKey.TryGetValue((key, typeof(TExtension)), out var obj))
                throw new KeyNotFoundException($"Host extension with key '{key}' and type '{typeof(TExtension)}' has not been registered.");

            return (TExtension)obj;
        }

        public bool TryGet<TExtension>(out TExtension result)
        {
            result = default;

            var has = byType.TryGetValue(typeof(TExtension), out var obj);
            if (has)
                result = (TExtension)obj;

            return has;
        }

        public bool TryGet<TExtension>(string key, out TExtension result)
        {
            result = default;

            var has = byKey.TryGetValue((key, typeof(TExtension)), out var obj);
            if (has)
                result = (TExtension)obj;

            return has;
        }

        public IEnumerable<(Type, object)> GetAll() =>
            byType.Select(p => (p.Key, p.Value));

        public void Add<TExtension>(TExtension extension, string key)
        {
            byKey.TryAdd((key, typeof(TExtension)), extension);
        }

        public void Add<TExtension>(TExtension extension)
        {
            byType.TryAdd(typeof(TExtension), extension);
        }
    }
}