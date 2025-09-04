using UnityEngine;

namespace FantasyColony.UI.Creator.Editing
{
    /// <summary>
    /// Simple metadata component to store per-item instructions/notes.
    /// </summary>
    public sealed class UIInstructionNote : MonoBehaviour
    {
        [TextArea(3, 10)] public string Note = string.Empty;
    }
}

