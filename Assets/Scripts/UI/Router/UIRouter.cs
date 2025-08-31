using System;
using System.Collections.Generic;
using UnityEngine;
using FantasyColony.Core;

namespace FantasyColony.UI.Router
{
    public sealed class UIRouter
    {
        private readonly Transform _parent;
        private readonly ServiceRegistry _services;
        private readonly Stack<IScreen> _stack = new();

        // Global access to the active router (useful for simple buttons like Restart)
        public static UIRouter Current { get; private set; }

        public UIRouter(Transform parent, ServiceRegistry services)
        {
            _parent = parent;
            _services = services;
            Current = this;
        }

        public void Push<T>() where T : IScreen, new()
        {
            var screen = new T();
            screen.Enter(_parent);
            _stack.Push(screen);
        }

        public void Pop()
        {
            if (_stack.Count == 0) return;
            var top = _stack.Pop();
            top.Exit();
        }

        // --- Restart helpers ---
        public void PopAll()
        {
            while (_stack.Count > 0) Pop();
        }

        public void ResetTo<T>() where T : IScreen, new()
        {
            PopAll();
            Push<T>();
        }
    }
}
