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
			foreach (var key in transformations.RemoveKeys)
			{
				if (String.IsNullOrWhiteSpace(key))
					throw new ArgumentException("Blank transformation key.", nameof(transformations));
				source.RemoveKey(SplitKey(key));
			}

			foreach (var transformation in transformations)
			{
				if (String.IsNullOrWhiteSpace(transformation.Key))
					throw new ArgumentException("Blank transformation key.", nameof(transformations));

				source.ReplaceKey(SplitKey(transformation.Key), transformation.Value);
			}

			return source.ToString();
		}

		private static string[] SplitKey(string key)
		{
			return key.Trim().Split('/').Select(x => x.Trim()).ToArray();
		}

		public void Dispose()
		{
		}
	}
}