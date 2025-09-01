using UnityEngine;

namespace FantasyColony
{
    public static class TransformPathExtensions
    {
        // Helper for readable logs (Editor only usage).
        public static string GetHierarchyPath(this Transform t)
        {
            if (t == null) return "<null>";
            System.Text.StringBuilder sb = new System.Text.StringBuilder(t.name);
            var p = t.parent;
            while (p != null)
            {
                sb.Insert(0, "/");
                sb.Insert(0, p.name);
                p = p.parent;
            }
            return sb.ToString();
        }
    }
}
