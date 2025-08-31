namespace FantasyColony.Core.Defs {
    /// <summary>
    /// Lightweight metadata about a def used for validation and migrations.
    /// </summary>
    public sealed class DefMeta {
        public string Type;            // e.g., FactionDef
        public string Id;              // canonical id string
        public string Path;            // source file path
        public string ModId;           // owning mod
        public string Schema;          // e.g., "FactionDef@1" or null
        public int SchemaVersion;      // parsed version (fallback from Schema)
    }
}
