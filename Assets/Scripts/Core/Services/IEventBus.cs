using System;
using System.Collections.Generic;

namespace FantasyColony.Core.Services
{
    public interface IEventBus
    {
        void Subscribe<T>(Action<T> handler);
        void Unsubscribe<T>(Action<T> handler);
        void Publish<T>(T evt);
    }

    public sealed class SimpleEventBus : IEventBus
    {
        private readonly Dictionary<Type, Delegate> _subs = new();

        public void Subscribe<T>(Action<T> handler)
        {
            if (_subs.TryGetValue(typeof(T), out var d))
                _subs[typeof(T)] = Delegate.Combine(d, handler);
            else
                _subs[typeof(T)] = handler;
        }

        public void Unsubscribe<T>(Action<T> handler)
        {
            if (_subs.TryGetValue(typeof(T), out var d))
            {
                var res = Delegate.Remove(d, handler);
                if (res == null) _subs.Remove(typeof(T));
                else _subs[typeof(T)] = res;
            }
        }

        public void Publish<T>(T evt)
        {
            if (_subs.TryGetValue(typeof(T), out var d) && d is Action<T> a)
            {
                a.Invoke(evt);
            }
        }
    }
}
