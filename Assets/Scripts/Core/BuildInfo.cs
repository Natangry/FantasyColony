namespace Core
{
    /// <summary>
    /// Static build info for diagnostics; can be auto-generated later by an Editor script.
    /// </summary>
    public static class BuildInfo
    {
        public const string Version = "0.1.0";
        public const string Commit = "local";
        public const string Unity = "AUTO"; // can be filled at runtime via UnityEngine.Application.unityVersion
    }
}

namespace Core.Services
{
    public static class BuildInfoRuntime
    {
        public static string Describe() => $"v{Core.BuildInfo.Version} ({Core.BuildInfo.Commit}) | Unity {UnityEngine.Application.unityVersion}";
    }
}
