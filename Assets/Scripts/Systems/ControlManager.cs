using System;
using UnityEngine;

// ReSharper disable Unity.InefficientPropertyAccess
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
        SelectionController.SelectOnly(Controlled); // pin selection to the controlled pawn
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

