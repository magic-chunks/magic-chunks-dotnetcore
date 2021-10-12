using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using MagicChunks.Helpers;

namespace MagicChunks.Core
{
    public class Transformer : ITransformer
    {
        
        private static readonly IEnumerable<Type> _documentTypes;

        static Transformer()
        {
            _documentTypes = typeof(Transformer)
                .GetTypeInfo()
                .Assembly
                .GetAllTypes(p => !p.IsAbstract && p.ImplementedInterfaces.Contains(typeof(IDocument)));
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

                    source.RemoveKey(TransformationKey.Split(key));
                }
            }

            foreach (var transformation in transformations)
            {
                var transformationKey = new TransformationKey(transformation.Key);

                switch (transformationKey.Type)
                {
                    case TransformationKeyType.Remove:
                        source.RemoveKey(transformationKey.Path);
                        break;
                    case TransformationKeyType.AddToArray:
                        source.AddElementToArray(transformationKey.Path, transformation.Value);
                        break;
                    case TransformationKeyType.Replace:
                        source.ReplaceKey(transformationKey.Path, transformation.Value);
                        break;
                    default:
                        throw new InvalidOperationException($"Action is not defined for {nameof(TransformationKeyType)}.{transformationKey.Type}");
                }
            }

            return source.ToString();
        }

        public void Dispose()
        {
        }
    }
}