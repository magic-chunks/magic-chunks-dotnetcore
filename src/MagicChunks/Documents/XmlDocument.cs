using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using MagicChunks.Core;
using MagicChunks.Helpers;

namespace MagicChunks.Documents
{
    public class XmlDocument : IDocument
    {
        private static readonly Regex AttributeFilterRegex = new Regex(@"(?<element>.+?)\[\s*\@(?<key>\w+)\s*\=\s*[\'\""]?(?<value>.+?)[\'\""]?\s*\]$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        protected readonly XDocument Document;

        public XmlDocument(string source)
        {
            try
            {
                Document = XDocument.Parse(source);
            }
            catch (XmlException ex)
            {
                throw new ArgumentException("Wrong document format", nameof(source), ex);
            }
        }

        public void ReplaceKey(string[] path, string value)
        {
            if ((path == null) || (path.Any() == false))
                throw new ArgumentException("Path is not speicified.", nameof(path));

            if (path.Any(String.IsNullOrWhiteSpace))
                throw new ArgumentException("There is empty items in the path.", nameof(path));

            XElement current = Document.Root;
            string documentNamespace = Document.Root?.Name.NamespaceName ?? String.Empty;

            if (current == null)
                throw new ArgumentException("Root element is not present.", nameof(path));

            if (String.Compare(current.Name.LocalName, path.First(), StringComparison.InvariantCultureIgnoreCase) != 0)
                throw new ArgumentException("Root element name does not match path.", nameof(path));

            current = FindPath(path.Skip(1).Take(path.Length - 2), current, documentNamespace);

            UpdateTargetElement(value, path.Last(), current, documentNamespace);
        }

        public void RemoveKey(string[] path)
        {
            if ((path == null) || (path.Any() == false))
                throw new ArgumentException("Path is not specified.", nameof(path));

            if (path.Any(String.IsNullOrWhiteSpace))
                throw new ArgumentException("There is empty items in the path.", nameof(path));

            XElement current = Document.Root;
            string documentNamespace = Document.Root?.Name.NamespaceName ?? String.Empty;

            if (current == null)
                throw new ArgumentException("Root element is not present.", nameof(path));

            if (String.Compare(current.Name.LocalName, path.First(), StringComparison.InvariantCultureIgnoreCase) != 0)
                throw new ArgumentException("Root element name does not match path.", nameof(path));

            current = FindPath(path.Skip(1).Take(path.Length - 2), current, documentNamespace);

            RemoveTargetElement(path.Last(), current, documentNamespace);
        }

        private static XElement FindPath(IEnumerable<string> path, XElement current, string documentNamespace)
        {
            foreach (string pathElement in path)
            {
                if (pathElement.StartsWith("@"))
                    throw new ArgumentException("Attribute element could be only at end of the path.", nameof(path));

                var attributeFilterMatch = AttributeFilterRegex.Match(pathElement);

                var currentElement = current?.GetChildElementByName(pathElement);

                if (attributeFilterMatch.Success)
                {
                    current = current.FindChildByAttrFilterMatch(attributeFilterMatch, documentNamespace);
                }
                else if (currentElement != null)
                {
                    current = currentElement;

                    if (current.HasElements == false)
                        current.Value = String.Empty;
                }
                else
                    current = current.CreateChildElement(documentNamespace, pathElement);
            }
            return current;
        }

        private static void UpdateTargetElement(string value, string targetElement, XElement current, string documentNamespace)
        {
            var attributeFilterMatch = AttributeFilterRegex.Match(targetElement);

            if (targetElement.StartsWith("@") == true)
            {   // Attriubte update
                current.SetAttributeValue(XName.Get(targetElement.TrimStart('@')), value);
            }
            else if (!attributeFilterMatch.Success)
            {   // Property update
                var elementToUpdate = current.GetChildElementByName(targetElement);

                if (elementToUpdate != null)
                    elementToUpdate.Value = value;
                else
                    current.Add(new XElement(XName.Get(targetElement, documentNamespace)) {Value = value});
            }
            else
            {   // Filtered element update
                current = current.FindChildByAttrFilterMatch(attributeFilterMatch, documentNamespace);
                current.Value = value;
            }
        }

        private static void RemoveTargetElement(string targetElement, XElement current, string documentNamespace)
        {
            var attributeFilterMatch = AttributeFilterRegex.Match(targetElement);

            if (targetElement.StartsWith("@") == true)
            {   // Attriubte update
                current.Attribute(XName.Get(targetElement.TrimStart('@')))
                    ?.Remove();
            }
            else if (!attributeFilterMatch.Success)
            {   // Property update
                var elementToRemove = current.GetChildElementByName(targetElement);
                elementToRemove.Remove();
            }
            else
            {   // Filtered element update
                current.FindChildByAttrFilterMatch(attributeFilterMatch, documentNamespace)
                    .Remove();
            }
        }

        public override string ToString()
        {
            return Document?.ToString(SaveOptions.None) ?? String.Empty;
        }

        public void Dispose()
        {
        }
    }
}