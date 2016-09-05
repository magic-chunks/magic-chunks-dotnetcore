using System.Collections.Generic;
using System.Linq;

namespace MagicChunks.Core
{
    public class TransformationCollection : Dictionary<string, string>
    {
        public TransformationCollection()
        {
            RemoveKeys = new List<string>();
        }

        public TransformationCollection(params string[] remove)
        {
            RemoveKeys = remove.ToList();
        }
        public List<string> RemoveKeys { get; }
    }
}