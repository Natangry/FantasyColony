using UnityEngine;

namespace Core.Input
{
    /// <summary>
    /// Placeholder router for the new Input System. No-ops if the package is missing.
    /// </summary>
    public static class InputRouter
    {
        public static void EnableMenuMap()
        {
#if ENABLE_INPUT_SYSTEM
            // Hook up your InputActionAsset and enable the UI/menu action map here.
#endif
        }

        public static void DisableAll()
        {
#if ENABLE_INPUT_SYSTEM
            // Disable action maps here.
#endif
        }
    }
}
