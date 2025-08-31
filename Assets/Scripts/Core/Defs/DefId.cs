using System;

namespace FantasyColony.Core.Defs {
    /// <summary>
    /// Canonical string id shape: modid.DefType.Name
    /// </summary>
    public struct DefId {
        public string Mod; public string Type; public string Name;
        public bool IsEmpty => string.IsNullOrEmpty(Mod) || string.IsNullOrEmpty(Type) || string.IsNullOrEmpty(Name);
        public override string ToString() => string.IsNullOrEmpty(Mod) ? $"{Type}.{Name}" : $"{Mod}.{Type}.{Name}";

        public static bool TryParse(string value, out DefId id) {
            id = default;
            if (string.IsNullOrEmpty(value)) return false;
            var parts = value.Split('.');
            if (parts.Length == 2) { // Type.Name (allowed for intra-pack refs)
                id = new DefId { Mod = string.Empty, Type = parts[0], Name = parts[1] };
                return true;
            }
            if (parts.Length == 3) {
                id = new DefId { Mod = parts[0], Type = parts[1], Name = parts[2] };
                return true;
            }
            return false;
        }
    }
}
