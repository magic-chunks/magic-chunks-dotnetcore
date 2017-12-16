using System;
using System.Collections.Generic;
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
        private static readonly Regex AttributeFilterRegex = new Regex(@"(?<element>.+?)\[\s*\@(?<key>[\w\:]+)\s*\=\s*[\'\""]?(?<value>.+?)[\'\""]?\s*\]$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex ProcessingInstructionsPathElementRegex = new Regex(@"^\?.+", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline);

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

        public void AddElementToArray(string[] path, string value)
        {
            if ((path == null) || (path.Any() == false))
                throw new ArgumentException("Path is not speicified.", nameof(path));

            if (path.Any(String.IsNullOrWhiteSpace))
                throw new ArgumentException("There is empty items in the path.", nameof(path));

            XElement current = Document.Root;
            string documentNamespace = Document.Root?.Name.NamespaceName ?? String.Empty;

            if (current == null)
                throw new ArgumentException("Root element is not present.", nameof(path));

            if (String.Compare(current.Name.LocalName, path.First(), StringComparison.OrdinalIgnoreCase) != 0)
                throw new ArgumentException("Root element name does not match path.", nameof(path));

            if (!path.Any(p => ProcessingInstructionsPathElementRegex.IsMatch(p)))
            {
                current = FindPath(path.Skip(1).Take(path.Length - 2), current, documentNamespace);
                UpdateTargetArrayElement(value, path.Last(), current, documentNamespace);
            }
            else
            {
                throw new NotSupportedException("Arrays are not supported for XML processing instructions");
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

            if (String.Compare(current.Name.LocalName, path.First(), StringComparison.OrdinalIgnoreCase) != 0)
                throw new ArgumentException("Root element name does not match path.", nameof(path));

            if (!path.Any(p => ProcessingInstructionsPathElementRegex.IsMatch(p)))
            {
                current = FindPath(path.Skip(1).Take(path.Length - 2), current, documentNamespace);
                UpdateTargetElement(value, path.Last(), current, documentNamespace);
            }
            else
            {
                throw new NotImplementedException();
            }
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

            if (String.Compare(current.Name.LocalName, path.First(), StringComparison.OrdinalIgnoreCase) != 0)
                throw new ArgumentException("Root element name does not match path.", nameof(path));

            if (!path.Any(p => ProcessingInstructionsPathElementRegex.IsMatch(p)))
            {
                current = FindPath(path.Skip(1).Take(path.Length - 2), current, documentNamespace);
                RemoveTargetElement(path.Last(), current, documentNamespace);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private static XElement FindPath(IEnumerable<string> path, XElement current, string documentNamespace)
        {
            foreach (string pathElement in path)
            {
                if (pathElement.StartsWith("@"))
                    throw new ArgumentException("Attribute element could be only at end of the path.", nameof(path));

                var currentElement = current?.GetChildElementByName(pathElement);

                var attributeFilterMatch = AttributeFilterRegex.Match(pathElement);
                if (attributeFilterMatch.Success)
                {
                    current = current.FindChildByAttrFilterMatch(attributeFilterMatch, documentNamespace);
                }
                else if (currentElement != null)
                {
                    current = currentElement;
                }
                else
                {
                    if (!current.HasElements)
                        current.SetValue("");
                    current = current.CreateChildElement(documentNamespace, pathElement);
                }
            }
            return current;
        }

        private static void UpdateTargetElement(string value, string targetElement, XElement current, string documentNamespace)
        {
            var attributeFilterMatch = AttributeFilterRegex.Match(targetElement);

            if (targetElement.StartsWith("@") == true)
            {   // Attriubte update
                current.SetAttributeValue(targetElement.TrimStart('@').GetNameWithNamespace(current, String.Empty), value.Replace("&quot;", @"""").Replace("&lt;", @"<").Replace("&gt;", @">"));
            }
            else if (!attributeFilterMatch.Success)
            {   // Property update
                var elementToUpdate = current.GetChildElementByName(targetElement);

                if (elementToUpdate != null)
                {
                    elementToUpdate.Value = value;
                }
                else
                {
                    if(!current.HasElements)
                        current.SetValue("");

                    current.Add(new XElement(targetElement.GetNameWithNamespace(current, documentNamespace)) {Value = value});
                }
            }
            else
            {   // Filtered element update
                current = current.FindChildByAttrFilterMatch(attributeFilterMatch, documentNamespace);
                current.Value = value;
            }
        }

        private static void UpdateTargetArrayElement(string value, string targetElement, XElement current, string documentNamespace)
        {
            var attributeFilterMatch = AttributeFilterRegex.Match(targetElement);

            if (!attributeFilterMatch.Success)
            {   // Property update
                var elementToUpdate = current.GetChildElementByName(targetElement);

                if (elementToUpdate != null)
                {
                    elementToUpdate.Add(XElement.Parse(value));
                }
                else
                {
                    if (!current.HasElements)
                        current.SetValue("");

                    elementToUpdate = new XElement(targetElement.GetNameWithNamespace(current, documentNamespace));
                    current.Add(elementToUpdate);

                    elementToUpdate.Add(XElement.Parse(value));
                }
            }
            else
            {   // Filtered element update
                current = current.FindChildByAttrFilterMatch(attributeFilterMatch, documentNamespace);
                current.Add(XElement.Parse(value));
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
            return Document?.ToStringWithDeclaration() ?? String.Empty;
        }

        public void Dispose()
        {
        }
    }
}