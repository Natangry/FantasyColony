using System;
using System.Globalization;

namespace FantasyColony.Core.Defs.Validation {
    internal static class TypeChecks {
        public static (bool ok, string err) Check(FieldSpec fs, string value, Defs.DefIndex index) {
            var t = (fs.type ?? "string").ToLowerInvariant();
            switch (t) {
                case "string": return CheckString(value, fs);
                case "id":     return CheckId(value);
                case "int":    return CheckInt(value, fs);
                case "float":  return CheckFloat(value, fs);
                case "bool":   return CheckBool(value);
                case "color":  return CheckColor(value);
                case "enum":   return CheckEnum(value, fs);
                case "defref": return CheckDefRef(value, fs, index);
                case "list":   return (true, null); // future: validate list items
                default:        return (true, null);
            }
        }

        private static (bool,string) CheckString(string v, FieldSpec fs) {
            if (v == null) return (false, "missing string");
            if (fs.minLength > 0 && v.Length < fs.minLength) return (false, $"string too short (min {fs.minLength})");
            return (true, null);
        }
        private static (bool,string) CheckId(string v) {
            if (string.IsNullOrEmpty(v)) return (false, "missing id");
            return (FantasyColony.Core.Defs.DefId.TryParse(v, out _)) ? (true, null) : (false, "id not well-formed");
        }
        private static (bool,string) CheckInt(string v, FieldSpec fs) {
            if (!int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i)) return (false, "not an int");
            if (fs.min != 0 && i < fs.min) return (false, $"int < min {fs.min}");
            if (fs.max != 0 && i > fs.max) return (false, $"int > max {fs.max}");
            return (true, null);
        }
        private static (bool,string) CheckFloat(string v, FieldSpec fs) {
            if (!float.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out var f)) return (false, "not a float");
            if (fs.min != 0 && f < fs.min) return (false, $"float < min {fs.min}");
            if (fs.max != 0 && f > fs.max) return (false, $"float > max {fs.max}");
            return (true, null);
        }
        private static (bool,string) CheckBool(string v) {
            if (!bool.TryParse(v, out _)) return (false, "not a bool");
            return (true, null);
        }
        private static (bool,string) CheckColor(string v) {
            if (string.IsNullOrEmpty(v)) return (false, "missing color");
            // Accept #RRGGBB or #RRGGBBAA
            if (v.StartsWith("#") && (v.Length == 7 || v.Length == 9)) return (true, null);
            return (false, "color must be #RRGGBB or #RRGGBBAA");
        }
        private static (bool,string) CheckEnum(string v, FieldSpec fs) {
            if (fs.values == null || fs.values.Length == 0) return (true, null);
            for (int i=0;i<fs.values.Length;i++) if (string.Equals(v, fs.values[i], StringComparison.Ordinal)) return (true, null);
            return (false, $"enum value '{v}' not in [{string.Join(",", fs.values)}]");
        }
        private static (bool,string) CheckDefRef(string v, FieldSpec fs, Defs.DefIndex index) {
            if (string.IsNullOrEmpty(v)) return (false, "missing defref");
            string type, id;
            var parts = v.Split('.');
            if (parts.Length == 2) { type = parts[0]; id = parts[1]; }
            else if (parts.Length == 3) { type = parts[1]; id = parts[2]; }
            else return (false, "defref must be Type.Id or modid.Type.Name");
            if (!string.IsNullOrEmpty(fs.target) && !string.Equals(type, fs.target, StringComparison.Ordinal))
                return (false, $"defref type '{type}' expected '{fs.target}'");
            var ok = index.Find(type, id) != null;
            return ok ? (true, null) : (false, $"missing target {type}.{id}");
        }
    }
}
