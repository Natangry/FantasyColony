using System;
using UnityEngine;

/// <summary>
/// Global "assume control" ownership. Exactly one pawn can be controlled at a time.
/// </summary>
[AddComponentMenu("Systems/Control Manager")]
public class ControlManager : MonoBehaviour
{
    public static SpritePawn Controlled { get; private set; }
    public static event Action<SpritePawn> OnControlledChanged;

    public static void AssumeControl(SpritePawn pawn)
    {
        if (pawn == null) return;
        if (Controlled == pawn)
        {
            // Already controlled; no change but still raise event for listeners if needed.
            try { OnControlledChanged?.Invoke(Controlled); } catch { }
            return;
        }
        // Release previous
        if (Controlled != null) Controlled.SetControlled(false);
        Controlled = pawn;
        Controlled.SetControlled(true);
        try { OnControlledChanged?.Invoke(Controlled); } catch { }
    }

    public static void ReleaseControl()
    {
        if (Controlled == null) return;
        Controlled.SetControlled(false);
        Controlled = null;
        try { OnControlledChanged?.Invoke(null); } catch { }
    }
}

