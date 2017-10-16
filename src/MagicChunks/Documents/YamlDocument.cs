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
    public class YamlDocument : IDocument
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
                }
                catch (YamlException ex)
                {
                    throw new ArgumentException("Wrong document format", nameof(source), ex);
                }
            }
        }

        public void AddElementToArray(string[] path, string value)
        {
            if ((path == null) || (path.Any() == false))
                throw new ArgumentException("Path is not speicified.", nameof(path));

            if (path.Any(String.IsNullOrWhiteSpace))
                throw new ArgumentException("There is empty items in the path.", nameof(path));

            Dictionary<object, object> current = Document;

            if (current == null)
                throw new ArgumentException("Root element is not present.", nameof(path));

            current = FindPath(path.Take(path.Length - 1), current);

            UpdateTargetArrayElement(current, path.Last(), value);
        }

        public void ReplaceKey(string[] path, string value)
        {
            if ((path == null) || (path.Any() == false))
                throw new ArgumentException("Path is not speicified.", nameof(path));

            if (path.Any(String.IsNullOrWhiteSpace))
                throw new ArgumentException("There is empty items in the path.", nameof(path));

            Dictionary<object, object> current = Document;

            if (current == null)
                throw new ArgumentException("Root element is not present.", nameof(path));

            current = FindPath(path.Take(path.Length - 1), current);

            UpdateTargetElement(current, path.Last(), value);
        }

        public void RemoveKey(string[] path)
        {
            if ((path == null) || (path.Any() == false))
                throw new ArgumentException("Path is not speicified.", nameof(path));

            if (path.Any(String.IsNullOrWhiteSpace))
                throw new ArgumentException("There is empty items in the path.", nameof(path));

            Dictionary<object, object> current = Document;

            if (current == null)
                throw new ArgumentException("Root element is not present.", nameof(path));

            current = FindPath(path.Take(path.Length - 1), current);
            current.Remove(path.Last());
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