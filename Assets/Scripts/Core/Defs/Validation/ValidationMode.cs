namespace FantasyColony.Core.Defs.Validation {
    /// <summary>
    /// Controls how strict validation is treated at runtime.
    /// Lenient is default for player builds; Strict is useful in Editor/CI.
    /// </summary>
    public enum ValidationMode {
        Lenient = 0,
        Strict = 1,
    }
}
