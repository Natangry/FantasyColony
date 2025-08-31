using System;
using FantasyColony.Core.Services;
using UnityEngine;

namespace FantasyColony.Core.Defs.Migrations {
    public static class MigrationEngine {
        /// <summary>
        /// Returns number of defs that were logically migrated (metadata-only for now).
        /// </summary>
        public static int Run(Defs.DefIndex index) {
            int migrated = 0;
            foreach (var m in index.Items) {
                var (ok, current) = SchemaRegistry.TryGetCurrentVersion(m.Type);
                if (!ok) continue; // unknown types are allowed
                if (m.SchemaVersion == 0) continue; // missing handled by validator; do not auto-set
                if (m.SchemaVersion < current) {
                    migrated++;
                    Debug.Log($"[Defs] Migrated {m.Type}.{m.Id} from v{m.SchemaVersion} to v{current}");
                    // Future: data transforms on DOM; for now, we only log logical migration
                }
            }
            return migrated;
        }
    }
}
