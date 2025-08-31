using System;
using System.Collections.Generic;

namespace FantasyColony.Core
{
    public sealed class ServiceRegistry
    {
        private readonly Dictionary<Type, object> _map = new();

        public void Register<T>(T impl) where T : class
        {
            _map[typeof(T)] = impl ?? throw new ArgumentNullException(nameof(impl));
        }

        public T Get<T>() where T : class
        {
            if (_map.TryGetValue(typeof(T), out var o) && o is T t)
                return t;
            throw new InvalidOperationException($"Service not registered: {typeof(T).Name}");
        }
    }
}
