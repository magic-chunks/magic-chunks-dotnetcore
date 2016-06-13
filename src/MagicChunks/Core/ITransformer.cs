using System;
using System.Collections.Generic;

namespace MagicChunks.Core
{
    /// <summary>
    /// Represents transformation object
    /// </summary>
    public interface ITransformer : IDisposable
    {
        /// <summary>
        /// List of possible document types
        /// </summary>
        IEnumerable<Type> DocumentTypes { get; }

        /// <summary>
        /// Transforms source document with specified transformations.
        /// It trying to detect document type automatically.
        /// </summary>
        /// <param name="source">Source document</param>
        /// <param name="transformations">List of transformations</param>
        /// <returns>Transformed document</returns>
        string Transform(string source, TransformationCollection transformations);

        /// <summary>
        /// Transforms source document with specified transformations
        /// </summary>
        /// <typeparam name="TDocument">Document type</typeparam>
        /// <param name="source">Source document</param>
        /// <param name="transformations">List of transformations</param>
        /// <returns>Transformed document</returns>
        string Transform<TDocument>(string source, TransformationCollection transformations)
            where TDocument: IDocument;

        /// <summary>
        /// Transforms source document with specified transformations
        /// </summary>
        /// <param name="documentType">Document handler type</param>
        /// <param name="source">Source document</param>
        /// <param name="transformations">List of transformations</param>
        /// <returns>Transformed document</returns>
        string Transform(Type documentType, string source, TransformationCollection transformations);

        /// <summary>
        /// Transforms source document with specified transformations
        /// </summary>
        /// <param name="source">Source document</param>
        /// <param name="transformations">List of transformations</param>
        /// <returns>Transformed document</returns>
        string Transform(IDocument source, TransformationCollection transformations);
    }
}