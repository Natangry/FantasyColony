using FantasyColony.Defs;

/// <summary>
/// Compatibility shim: legacy code references VisualDef. We now use Visual2DDef.
/// This shim keeps existing code compiling while we migrate call sites.
/// </summary>
[System.Serializable]
public class VisualDef : Visual2DDef
{
    // Intentionally empty. Inherits all fields from Visual2DDef.
}



