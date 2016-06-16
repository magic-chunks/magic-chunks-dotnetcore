using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MagicChunks.Core
{
    public class Transformer : ITransformer
    {
        private static readonly IEnumerable<Type> _documentTypes;

        static Transformer()
        {
            _documentTypes = typeof(Transformer)
                .Assembly
                .GetTypes()
                .Where(p => typeof(IDocument).IsAssignableFrom(p) && !p.IsAbstract)
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
            var document = (TDocument) Activator.CreateInstance(typeof(TDocument), source);
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
            foreach (var transformation in transformations)
            {
                if (String.IsNullOrWhiteSpace(transformation.Key) == false)
                    source.ReplaceKey(transformation.Key.Trim().Split('/').Select(x => x.Trim()).ToArray(), transformation.Value);
                else
                    throw new ArgumentException("Wrong transformation key.", nameof(transformations));
            }

            return source.ToString();
        }

        public void Dispose()
        {
        }
    }
}