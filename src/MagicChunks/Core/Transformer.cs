using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MagicChunks.Core
{
    public class Transformer : ITransformer
    {
        private static readonly Regex RemoveEndingRegex = new Regex(@"\`\d+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex RemoveArrayEndingRegex = new Regex(@"\[\]$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly IEnumerable<Type> _documentTypes;

        static Transformer()
        {
            _documentTypes = typeof(Transformer)
                .GetTypeInfo()
                .Assembly
                .DefinedTypes
                .Where(p => !p.IsAbstract && p.ImplementedInterfaces.Contains(typeof(IDocument)))
                .Select(p => p.AsType())
                .ToArray();

        }

        public IEnumerable<Type> DocumentTypes => _documentTypes;

        public string Transform(string source, TransformationCollection transformations)
        {

            foreach (var documentType in DocumentTypes)
            {
                IDocument document;
                try
                {
                    document = (IDocument)Activator.CreateInstance(documentType, source);
                }
                catch (TargetInvocationException ex)
                {
                    if (ex.InnerException.GetType() == typeof(ArgumentException))
                        continue;

                    throw;
                }

                return Transform(document, transformations);
            }

            throw new ArgumentException("Unknown document type.", nameof(source));
        }

        public string Transform<TDocument>(string source, TransformationCollection transformations) where TDocument : IDocument
        {
            var document = (TDocument)Activator.CreateInstance(typeof(TDocument), source);
            return Transform(document, transformations);
        }

        public string Transform(Type documentType, string source, TransformationCollection transformations)
        {
            IDocument document;
            try
            {
                document = (IDocument)Activator.CreateInstance(documentType, source);
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException.GetType() == typeof(ArgumentException))
                    throw new ArgumentException("Unknown document type.", nameof(source), ex);

                throw;
            }

            return Transform(document, transformations);
        }

        public string Transform(IDocument source, TransformationCollection transformations)
        {
            if (transformations.KeysToRemove != null)
            {
                foreach (var key in transformations.KeysToRemove)
                {
                    if (String.IsNullOrWhiteSpace(key))
                        throw new ArgumentNullException("Transformation key is empty.", nameof(transformations));

                    source.RemoveKey(SplitKey(key));
                }
            }

            foreach (var transformation in transformations)
            {
                if (String.IsNullOrWhiteSpace(transformation.Key))
                    throw new ArgumentException("Transformation key is empty.", nameof(transformations));

                if (transformation.Key.StartsWith("#"))
                {
                    source.RemoveKey(SplitKey(transformation.Key.TrimStart('#')));
                }
                else if (RemoveEndingRegex.Replace(transformation.Key, String.Empty).EndsWith("[]"))
                {
                    source.AddElementToArray(SplitKey(transformation.Key), transformation.Value);
                }
                else
                {
                    source.ReplaceKey(SplitKey(transformation.Key), transformation.Value);
                }
            }

            return source.ToString();
        }

        private static string[] SplitKey(string key)
        {
            return RemoveArrayEndingRegex.Replace(RemoveEndingRegex.Replace(key.Trim(), String.Empty), String.Empty).Split('/').Select(x => x.Trim()).ToArray();
        }

        public void Dispose()
        {
        }
    }
}