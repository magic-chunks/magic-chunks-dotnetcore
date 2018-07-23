using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MagicChunks.Core;
using MagicChunks.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MagicChunks.Documents
{
    public class JsonDocument : Document, IDocument
    {
        private static readonly Regex JsonObjectRegex = new Regex(@"^{.+}$$", RegexOptions.CultureInvariant | RegexOptions.Compiled);
        private static readonly Regex NodeIndexEndingRegex = new Regex(@"\[\d+\]$", RegexOptions.CultureInvariant | RegexOptions.Compiled);
        private static readonly Regex NodeValueEndingRegex = new Regex(@"\[\@.+\=.+\]$", RegexOptions.CultureInvariant | RegexOptions.Compiled);
        protected readonly JObject Document;

        public JsonDocument(string source)
        {
            try
            {
                Document = (JObject)JsonConvert.DeserializeObject(source);

                if (Document.Root == null)
                    throw new ArgumentException("Root element is not present.", nameof(source));
            }
            catch (JsonReaderException ex)
            {
                throw new ArgumentException("Wrong document format", nameof(source), ex);
            }
        }

        public void AddElementToArray(string[] path, string value)
        {
            (var targets, var pathEnding) = Process(path);

            foreach (var target in targets)
            {
                UpdateTargetArrayElement(target, pathEnding, value);
            }
        }

        public void ReplaceKey(string[] path, string value)
        {
            (var targets, var pathEnding) = Process(path);

            foreach (var target in targets)
            {
                UpdateTargetElement(target, pathEnding, value);
            }
        }

        public void RemoveKey(string[] path)
        {
            (var targets, var pathEnding) = Process(path);

            if (NodeIndexEndingRegex.IsMatch(pathEnding) || NodeValueEndingRegex.IsMatch(pathEnding))
            {
                // Remove item from array
                foreach (var target in targets)
                {
                    foreach (var item in target.GetChildPropertyValue(pathEnding))
                    {
                        item.Remove();
                    }
                }
            }
            else
            {
                // Remove property
                foreach (var target in targets)
                {
                    target.Remove(pathEnding);
                }
            }
        }

        private (IEnumerable<JObject>, string) Process(string[] path)
        {
            ValidatePath(path);

            var targets = FindPath(path.Take(path.Length - 1), (JObject)Document.Root);
            var pathEnding = path.Last();

            return (targets, pathEnding);
        }

        private static IEnumerable<JObject> FindPath(IEnumerable<string> path, JObject current)
        {
            var pathElements = new Queue<string>(path);

            foreach (string pathElement in path)
            {
                pathElements.Dequeue();

                var element = current.GetChildPropertyValue(pathElement).FirstOrDefault();
                if (element is JObject)
                {
                    current = (JObject)element;
                }
                else if (element is JArray)
                {
                    return ((JArray)element).SelectMany(arrayElement => FindPath(pathElements.ToArray(), arrayElement as JObject)).ToArray();
                }
                else
                {
                    current[pathElement] = new JObject();
                    current = (JObject)current[pathElement];
                }
            }
            return new JObject[] { current };
        }

        private static void UpdateTargetArrayElement(JObject current, string targetElementName, string value)
        {
            var targetElement = current.GetChildProperty(targetElementName) as JProperty;
            if ((targetElement != null) && (targetElement is JProperty) && (targetElement.Value is JArray))
            {
                if (JsonObjectRegex.IsMatch(value.Trim()))
                {
                    ((JArray)targetElement.Value).Add(JsonConvert.DeserializeObject(value));
                }
                else
                {
                    ((JArray)targetElement.Value).Add(value);
                }
            }
            else if (targetElement != null)
                throw new FormatException("Target element is not array.");
            else
            {
                var array = new JArray();
                if (JsonObjectRegex.IsMatch(value.Trim()))
                {
                    array.Add(JsonConvert.DeserializeObject(value));
                }
                else
                {
                    array.Add(value);
                }
                current.Add(targetElementName, array);
            }
        }

        private static void UpdateTargetElement(JObject current, string targetElementName, string value)
        {
            var targetElement = current.GetChildProperty(targetElementName);
            if (targetElement is JProperty)
                ((JProperty)targetElement).Value = value;
            else if (targetElement is JObject)
            {
                var targetValue = JsonConvert.DeserializeObject(value) as JObject;
                if (targetValue != null)
                    ((JObject)targetElement).Replace(targetValue);
                else
                    throw new ArgumentException("Value is not valid JSON object.", nameof(value));
            }
            else
                current.Add(targetElementName, value);
        }

        public override string ToString()
        {
            return Document?.ToString() ?? String.Empty;
        }

        public void Dispose()
        {
        }

    }
}