using System;
using System.Collections;

namespace FantasyColony.Boot
{
    // Minimal bootstrap pipeline. Replace yields with real work as the project grows.
    public static class AppBootstrap
    {
        // setPhase updates the BootScreen subtitle
        public static IEnumerator Run(Action<string> setPhase)
        {
            setPhase?.Invoke("Loading defs…");
            yield return null;

            setPhase?.Invoke("Initializing…");
            yield return null;

            setPhase?.Invoke("Building assets…");
            yield return null;

            setPhase?.Invoke(string.Empty);
        }
    }
}
