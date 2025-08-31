using System;
using System.Collections.Generic;

namespace FantasyColony.Core.Services
{
    /// <summary>
    /// Extremely simple definition registry keyed by type then id.
    /// Stores file paths for now; later this can hold parsed objects.
    /// </summary>
    public class DefRegistry
    {
        private static DefRegistry _instance;
        public static DefRegistry Instance => _instance ?? (_instance = new DefRegistry());

        private readonly Dictionary<string, Dictionary<string, string>> _map = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        public int Count { get; private set; }

        public void Add(string type, string id, string filePath)
        {
            if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(id)) return;
            if (!_map.TryGetValue(type, out var inner))
            {
                inner = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                _map[type] = inner;
            }
            inner[id] = filePath ?? string.Empty;
            Count++;
        }

        public bool TryGetPath(string type, string id, out string path)
        {
            path = null;
            if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(id)) return false;
            if (_map.TryGetValue(type, out var inner))
            {
                return inner.TryGetValue(id, out path);
            }
            return false;
        }

        public IEnumerable<string> Types() => _map.Keys;

        public IEnumerable<string> Ids(string type)
        {
            if (_map.TryGetValue(type, out var inner)) return inner.Keys;
            return Array.Empty<string>();
        }
    }
}
