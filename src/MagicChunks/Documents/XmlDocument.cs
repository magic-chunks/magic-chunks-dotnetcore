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
        private static readonly Regex AttributePathElementRegex = new Regex(@"^\@.+", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex AttributeNodeRegex = new Regex(@"(?<attrName>\w+)\s*\=\s*\""(?<attrValue>.+?)\""", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline);

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
                current = FindPath(path.Skip(1).Take(path.Length - 2), current, documentNamespace) as XElement;
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
                current = FindPath(path.Skip(1).Take(path.Length - 2), current, documentNamespace) as XElement;
                UpdateTargetElement(value, path.Last(), current, documentNamespace);
            }
            else
            {
                if (path.Take(path.Length - 2).Any(p => ProcessingInstructionsPathElementRegex.IsMatch(p)))
                {
                    throw new ArgumentException("Processing instruction could not contain nested elements.", nameof(path));
                }

                if (!ProcessingInstructionsPathElementRegex.IsMatch(path.Skip(path.Length - 2).First()))
                {
                    throw new ArgumentException("To update processing instruction you should point attribute name.", nameof(path));
                }

                if (!AttributePathElementRegex.IsMatch(path.Last()))
                {
                    throw new ArgumentException("To update processing instruction you should point attribute name.", nameof(path));
                }

                var processingInstruction = FindPath(path.Skip(1).Take(path.Length - 2), current, documentNamespace) as XProcessingInstruction;
                UpdateProcessingInstruction(value, path.Last().TrimStart('@'), processingInstruction, documentNamespace);
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
                current = FindPath(path.Skip(1).Take(path.Length - 2), current, documentNamespace) as XElement;
                RemoveTargetElement(path.Last(), current, documentNamespace);
            }
            else
            {
                var processingInstruction = FindPath(path.Skip(1).Take(path.Length - 2), current, documentNamespace) as XProcessingInstruction;

                // Remove whole processing instruction (not just single attribute)
                if (processingInstruction == null)
                    processingInstruction = FindPath(path.Skip(1).Take(path.Length - 1), current, documentNamespace) as XProcessingInstruction;

                RemoveProcessingInstruction(path.Last(), processingInstruction, documentNamespace);
            }
        }

        private static XNode FindPath(IEnumerable<string> path, XElement current, string documentNamespace)
        {
            XNode result = current;

            foreach (string pathElement in path)
            {
                if (pathElement.StartsWith("@"))
                    throw new ArgumentException("Attribute element could be only at end of the path.", nameof(path));

                var currentElement = !ProcessingInstructionsPathElementRegex.IsMatch(pathElement) ? (result as XElement)?.GetChildElementByName(pathElement) as XNode : (result as XElement)?.GetChildProcessingInstructionByName(pathElement.TrimStart('?'));

                var attributeFilterMatch = AttributeFilterRegex.Match(pathElement);
                if (attributeFilterMatch.Success)
                {
                    if (!ProcessingInstructionsPathElementRegex.IsMatch(pathElement))
                    {
                        result = (result as XElement).FindChildByAttrFilterMatch(attributeFilterMatch, documentNamespace);
                    }
                    else
                    {
                        result = (result as XElement).FindProcessingInstructionByAttrFilterMatch(attributeFilterMatch, documentNamespace);
                    }
                }
                else if (currentElement != null)
                {
                    result = currentElement;
                }
                else
                {
                    if (!(result as XElement).HasElements)
                        (result as XElement).SetValue("");

                    if (!ProcessingInstructionsPathElementRegex.IsMatch(pathElement))
                        result = (result as XElement).CreateChildElement(documentNamespace, pathElement);
                    else
                        result = (result as XElement).CreateChildProcessingInstruction(documentNamespace, pathElement.TrimStart('?'));
                }
            }

            return result;
        }

        private static void UpdateTargetElement(string value, string targetElement, XElement current, string documentNamespace)
        {
            var attributeFilterMatch = AttributeFilterRegex.Match(targetElement);

            if (targetElement.StartsWith("@") == true)
            {   // Attriubte update
                current.SetAttributeValue(targetElement.TrimStart('@').GetNameWithNamespace(current, String.Empty), value.Replace("&quot;", @"""").Replace("&lt;", @"<").Replace("&gt;", @">").Replace("&amp;quot;", @"&quot;").Replace("&amp;lt;", @"&lt;").Replace("&amp;gt;", @"&gt;"));
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
                    if (!current.HasElements)
                        current.SetValue("");

                    current.Add(new XElement(targetElement.GetNameWithNamespace(current, documentNamespace)) { Value = value });
                }
            }
            else
            {   // Filtered element update
                current = current.FindChildByAttrFilterMatch(attributeFilterMatch, documentNamespace);
                current.Value = value;
            }
        }

        private static void UpdateProcessingInstruction(string value, string targetElement, XProcessingInstruction current, string documentNamespace)
        {
            var valuesToReplace = AttributeNodeRegex.Matches(current.Data).OfType<Match>().Where(a =>
            {
                var eAttrName = a.Groups["attrName"]?.Value;
                var eAttrValue = a.Groups["attrValue"]?.Value;

                return String.Compare(eAttrName, targetElement, StringComparison.OrdinalIgnoreCase) == 0;
            }).Select(m => m.Value).ToArray();

            if (valuesToReplace.Any())
            {
                foreach (var replaceValue in valuesToReplace)
                {
                    current.Data = current.Data.Replace(replaceValue, $"{targetElement}=\"{value}\"");
                }
            }
            else
            {
                current.Data += (!string.IsNullOrEmpty(current.Data) ? " " : string.Empty) + $"{targetElement}=\"{value}\"";
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

        private static void RemoveProcessingInstruction(string targetElement, XProcessingInstruction current, string documentNamespace)
        {
            if (ProcessingInstructionsPathElementRegex.IsMatch(targetElement))
            {
                // remove  whole processing restriction
                current.Remove();
            }
            else if (AttributePathElementRegex.IsMatch(targetElement))
            {
                // remove attribute from processing instruction
                var valuesToRemove = AttributeNodeRegex.Matches(current.Data).OfType<Match>().Where(a =>
                {
                    var eAttrName = a.Groups["attrName"]?.Value;

                    return String.Compare(eAttrName, targetElement.TrimStart('@'), StringComparison.OrdinalIgnoreCase) == 0;
                }).Select(m => m.Value).ToArray();

                if (valuesToRemove.Any())
                {
                    foreach (var value in valuesToRemove)
                    {
                        current.Data = current.Data.Replace(value, string.Empty);
                    }
                }
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