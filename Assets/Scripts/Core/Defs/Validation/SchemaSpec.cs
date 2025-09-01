using System;
using System.Collections.Generic;

namespace FantasyColony.Core.Defs.Validation {
    /// <summary>
    /// JSON-deserializable schema spec for a single Def type & version.
    /// Stored as StreamingAssets/Defs/Schemas/<DefType>@<version>.schema.json
    /// </summary>
    [Serializable]
    public sealed class SchemaSpec {
        public string type;      // e.g., "FactionDef"
        public int version;      // e.g., 1
        public string[] required; // names of required fields
        public List<FieldSpec> fields; // all declared fields
    }

    [Serializable]
    public sealed class FieldSpec {
        public string name;      // field name as it appears in XML
        public string kind;      // "attr" or "elem"
        public string type;      // string|int|float|bool|color|enum|defref|id|list
        public string of;        // element type for lists (optional)
        public string target;    // for defref: target DefType
        public string[] values;  // for enum: allowed values
        public int min;          // numeric bounds (optional)
        public int max;
        public int minLength;    // for string
        [NonSerialized] public bool hasMin; // runtime: whether 'min' was present in JSON
        [NonSerialized] public bool hasMax; // runtime: whether 'max' was present in JSON
        public string @default;  // optional default as text
    }
}
