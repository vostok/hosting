using System;
using System.Collections.Concurrent;
using Vostok.Hosting.Abstractions;

namespace Vostok.Hosting.Components.HostExtensions
{
    internal class HostExtensions : IVostokHostExtensions
    {
        private readonly ConcurrentDictionary<Type, object> byType = new ConcurrentDictionary<Type, object>();
        private readonly ConcurrentDictionary<(string, Type), object> byKey = new ConcurrentDictionary<(string, Type), object>();

        public TExtension Get<TExtension>() =>
            (TExtension)byType[typeof(TExtension)];

        public TExtension Get<TExtension>(string key) =>
            (TExtension)byKey[(key, typeof(TExtension))];

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