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

        /// <summary>
        /// Non-throwing lookup for fail-soft flows (player builds).
        /// </summary>
        public bool TryGet<T>(out T service) where T : class
        {
            if (_map.TryGetValue(typeof(T), out var o) && o is T t)
            {
                service = t;
                return true;
            }
            service = null;
            return false;
        }

        /// <summary>
        /// Returns true if a service of type T is registered.
        /// </summary>
        public bool Has<T>() where T : class => _map.ContainsKey(typeof(T));

        /// <summary>
        /// Optional unregistration helper. Returns true if removed.
        /// </summary>
        public bool Unregister<T>() where T : class => _map.Remove(typeof(T));
    }
}
