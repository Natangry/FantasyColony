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

        public UIRouter(Transform parent, ServiceRegistry services)
        {
            _parent = parent;
            _services = services;
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
    }
}
