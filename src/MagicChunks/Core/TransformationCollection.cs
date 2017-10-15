using System.Collections.Generic;

namespace MagicChunks.Core
{
    public class TransformationCollection : Dictionary<string, string>
    {
        public TransformationCollection()
        {
        }

        public TransformationCollection(params string[] keysToRemove)
        {
            KeysToRemove = keysToRemove;
        }

        public IEnumerable<string> KeysToRemove { get; set; }
    }
}