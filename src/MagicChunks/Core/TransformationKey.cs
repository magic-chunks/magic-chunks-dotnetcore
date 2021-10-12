using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace MagicChunks.Core
{
    public enum TransformationKeyType
    {
        Replace,
        AddToArray,
        Remove
    }

    public class TransformationKey
    {
        private static readonly Regex RemoveEndingRegex = new Regex(@"\`\d+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex RemoveArrayEndingRegex = new Regex(@"\[\]$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex KeyChunkRegex = new Regex(@"\s?([^\/\[\]]+)(\[.+\])*\s?", RegexOptions.Compiled | RegexOptions.CultureInvariant);


        public TransformationKey(string transformationKey)
        {
            if (String.IsNullOrWhiteSpace(transformationKey))
                throw new ArgumentException("Transformation key is empty.", nameof(transformationKey));

            if (transformationKey.StartsWith("#"))
            {
                Type = TransformationKeyType.Remove;
                Path = Split(transformationKey.TrimStart('#'));
                return;
            }

            if (RemoveEndingRegex.Replace(transformationKey, String.Empty).EndsWith("[]"))
            {
                Type = TransformationKeyType.AddToArray;
                Path = Split(RemoveArrayEndingRegex.Replace(RemoveEndingRegex.Replace(transformationKey, String.Empty), String.Empty));
                return;
            }

            Type = TransformationKeyType.Replace;
            Path = Split(transformationKey);
            
        }

        public TransformationKeyType Type { get; }

        public string[] Path { get; }

        public static string[] Split(string trasformationKey)
        {
            var splits = KeyChunkRegex.Matches(trasformationKey)
                .Cast<Match>().Select(x => x.Value.Trim());

            return splits.ToArray();
        }
    }
}