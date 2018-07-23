using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MagicChunks.Core
{
    public class Document
    {
        protected virtual void ValidatePath(string[] path)
        {
            if ((path == null) || (path.Any() == false))
                throw new ArgumentException("Path is not speicified.", nameof(path));

            if (path.Any(String.IsNullOrWhiteSpace))
                throw new ArgumentException("There is at least one empty segment in the path.", nameof(path));
        }
    }
}
