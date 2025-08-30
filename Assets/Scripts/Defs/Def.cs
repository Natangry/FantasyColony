using System;

namespace FantasyColony.Defs
{
    /// <summary>
    /// Base class for all data-driven defs. All content lives in XML; behaviors live in C#.
    /// </summary>
    [Serializable]
    public abstract class Def
    {
        public string defName;
        public string modId; // set by loader based on the mod directory
        public string[] tags; // optional authoring tags

        public virtual string GetKindName() => GetType().Name;
    }
}

namespace FantasyColony.Defs
{
    /// <summary>
    /// Simple type discriminator for debugging/logging.
    /// </summary>
    public enum DefType
    {
        Unknown = 0,
        Visual2D = 1,
        Building = 2,
    }
}

