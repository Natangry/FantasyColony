namespace FantasyColony.Core.Services
{
    public interface IConfigService
    {
        string Get(string key, string fallback = "");
        void Set(string key, string value);
        void Save();
    }

    /// <summary>
    /// Minimal no-op config for early boot. Replace later with a JSON-backed service.
    /// </summary>
    public sealed class DummyConfigService : IConfigService
    {
        public string Get(string key, string fallback = "") => fallback;
        public void Set(string key, string value) { }
        public void Save() { }
    }
}
