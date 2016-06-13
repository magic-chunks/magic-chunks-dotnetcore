using System.Collections.Generic;

namespace MagicChunks.Documents
{
    public class IgnoreCaseComparer : IEqualityComparer<object>
    {
        public bool Equals(object x, object y)
        {
            if (x is string && y is string)
                return ((string) x).ToLowerInvariant() == ((string) y).ToLowerInvariant();

            return x == y;
        }

        public int GetHashCode(object obj)
        {
            var s = obj as string;
            return s != null ? s.ToLowerInvariant().GetHashCode() : obj.GetHashCode();
        }
    }
}