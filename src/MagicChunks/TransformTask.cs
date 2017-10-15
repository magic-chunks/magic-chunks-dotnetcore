using System;
using System.IO;
using System.Linq;
using MagicChunks.Core;

//https://stackoverflow.com/questions/43165788/merging-net-standard-assemblies
namespace MagicChunks
{
    public static class TransformTask
    {
        public static void Transform(string sourcePath, string targetPath, TransformationCollection transformation)
        {
            Transform(null, sourcePath, targetPath, transformation);
        }

        public static void Transform(string type, string sourcePath, string targetPath, TransformationCollection transformation)
        {
            if (File.Exists(sourcePath) == false)
                throw new ArgumentException($"File \"{sourcePath}\" does not exists.", nameof(sourcePath));

            string target;
            if (String.IsNullOrWhiteSpace(targetPath) == false)
            {
                try
                {
                    target = Path.GetFullPath(targetPath);
                }
                catch (ArgumentException ex)
                {
                    throw new ArgumentException($"Path \"{targetPath}\" is incorrect.", nameof(targetPath), ex);
                }
                catch (IOException ex)
                {
                    throw new ArgumentException($"Path \"{targetPath}\" is incorrect.", nameof(targetPath), ex);
                }
            }
            else
            {
                target = sourcePath;
            }

            var source = File.ReadAllText(sourcePath);
            string result;

            ITransformer transformer = new Transformer();

            if (String.IsNullOrWhiteSpace(type) == false)
            {
                var document = transformer.DocumentTypes.FirstOrDefault(x => x.Name == type + "Document");
                if (document == null)
                    throw new ArgumentException($"Wrong document type: {type}", nameof(type));

                result = transformer.Transform(document, source, transformation);
            }
            else
            {
                result = transformer.Transform(source, transformation);
            }

            File.WriteAllText(target, result);
        }
    }
}