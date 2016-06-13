using System;

namespace MagicChunks.Core
{
    public interface IDocument : IDisposable
    {
        void ReplaceKey(string[] path, string value);
    }
}