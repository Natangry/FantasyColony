using System.Collections.Generic;

namespace FantasyColony.Core.Defs.Migrations {
    /// <summary>
    /// Known def types and their current schema versions.
    /// Extend as new def families are introduced.
    /// </summary>
    public static class SchemaRegistry {
        private static readonly Dictionary<string, int> _current = new Dictionary<string, int> {
            { "FactionDef", 1 },
            { "ItemDef", 1 },
            { "BiomeDef", 1 },
            { "RecipeDef", 1 },
        };

        public static (bool ok, int version) TryGetCurrentVersion(string defType) {
            if (_current.TryGetValue(defType, out var v)) return (true, v);
            return (false, 0);
        }
    }
}
