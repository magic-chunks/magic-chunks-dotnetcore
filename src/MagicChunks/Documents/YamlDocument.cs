using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MagicChunks.Core;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MagicChunks.Documents
{
    public class YamlDocument : Document, IDocument
    {
        protected readonly Dictionary<object, object> Document;

        public YamlDocument(string source)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .WithObjectFactory(new CustomObjectFactory())
                .Build();

            using (var reader = new StringReader(source))
            {
                try
                {
                    Document = (Dictionary<object, object>)deserializer.Deserialize(reader);

                    if (Document == null)
                        throw new ArgumentException("Root element is not present.", nameof(source));

                }
                catch (YamlException ex)
                {
                    throw new ArgumentException("Wrong document format", nameof(source), ex);
                }
            }
        }

        public void AddElementToArray(string[] path, string value)
        {
            UpdateTargetArrayElement(Process(path), path.Last(), value);
        }

        public void ReplaceKey(string[] path, string value)
        {
            UpdateTargetElement(Process(path), path.Last(), value);
        }

        public void RemoveKey(string[] path)
        {
            Process(path).Remove(path.Last());
        }

        private Dictionary<object, object> Process(string[] path)
        {
            ValidatePath(path);
            var current = FindPath(path.Take(path.Length - 1), Document);
            return current;
        }

        private static Dictionary<object, object> FindPath(IEnumerable<string> path, Dictionary<object, object> current)
        {
            foreach (string pathElement in path)
            {
                object pathElementValue;
                if (current.TryGetValue(pathElement, out pathElementValue) && (pathElementValue is Dictionary<object, object>))
                {
                    current = (Dictionary<object, object>)pathElementValue;
                }
                else
                {
                    var newElement = new Dictionary<object, object>();
                    current[pathElement] = newElement;
                    current = newElement;
                }
            }
            return current;
        }

        private static void UpdateTargetElement(Dictionary<object, object> current, string targetElementName, string value)
        {
            current[targetElementName] = value;
        }

        private static void UpdateTargetArrayElement(Dictionary<object, object> current, string targetElementName, string value)
        {
            
            if (current.ContainsKey(targetElementName) && (current[targetElementName] is List<object>))
            {
                ((List<object>)current[targetElementName]).Add(value);
            }
            else if (!current.ContainsKey(targetElementName))
            {
                current[targetElementName] = new List<object>();
                ((List<object>)current[targetElementName]).Add(value);
            }
            else
            {
                throw new FormatException("Target element is not array.");
            }
        }

        public override string ToString()
        {
            using (var writer = new StringWriter())
            {
                var serializer = new Serializer();
                serializer.Serialize(writer, Document);

                return writer.ToString();
            }
        }

        public void Dispose()
        {
        }
    }
}