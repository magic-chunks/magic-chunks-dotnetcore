using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace MagicChunks.Helpers
{
    public static class XmlExtensions
    {
        private static readonly Regex NodeIndexEndingRegex = new Regex(@"\[\d+\]$", RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public static string ToStringWithDeclaration(this XDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return document.ToStringWithDeclaration(SaveOptions.None);
        }

        public static string ToStringWithDeclaration(this XDocument document, SaveOptions options)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            var newLine = (options & SaveOptions.DisableFormatting) == SaveOptions.DisableFormatting ? string.Empty : Environment.NewLine;

            return
                document.Declaration == null ?
                    document.ToString(options) :
                    document.Declaration + newLine + document.ToString(options);
        }

        public static XName GetNameWithNamespace(this string name, XElement element, string defaultNamespace)
        {
            XName result;
            if (name.Contains(':') == false)
                result = XName.Get(name, defaultNamespace);
            else
            {
                var attributeNameParts = name.Split(':');
                var attributeNamespace = element.GetNamespaceOfPrefix(attributeNameParts[0]);
                if (attributeNamespace != null)
                    result = XName.Get(attributeNameParts[1], attributeNamespace.NamespaceName);
                else
                    result = XName.Get(attributeNameParts[1], defaultNamespace);
            }

            return result;
        }

        public static XElement GetChildElementByName(this XElement source, string name)
        {
            if (!NodeIndexEndingRegex.IsMatch(name))
            {
                bool isElementNameWithNamespace = name.IndexOf(':') > 0 && !(name.Split(':')[0].Contains("'") || (name.Split(':')[0].Contains(@"""")));

                return source?.Elements()
                    .FirstOrDefault(e => !isElementNameWithNamespace ?
                                            String.Compare(e.Name.LocalName, name, StringComparison.OrdinalIgnoreCase) == 0 :
                                            e.Name == name.GetNameWithNamespace(source, String.Empty));
            }
            else
            {
                string nodeName;
                int nodeIndex;

                try
                {
                    nodeName = NodeIndexEndingRegex.Replace(name, String.Empty);
                    nodeIndex = int.Parse(NodeIndexEndingRegex.Match(name).Value.Trim('[', ']'));

                    if (nodeIndex < 0)
                        throw new ArgumentException("Index should be greater than 0.");
                }
                catch (ArgumentException ex)
                {
                    throw new ArgumentException($"Wrong element name: {name}", ex);
                }
                catch (FormatException ex)
                {
                    throw new ArgumentException($"Wrong element name: {name}", ex);
                }
                catch (OverflowException ex)
                {
                    throw new ArgumentException($"Wrong element name: {name}", ex);
                }

                var elements = source?.Elements()
                        .Where(e => name.IndexOf(':') == -1 ?
                                String.Compare(e.Name.LocalName, nodeName, StringComparison.OrdinalIgnoreCase) == 0 :
                                e.Name == nodeName.GetNameWithNamespace(source, String.Empty));

                return elements.Skip(nodeIndex).FirstOrDefault();
            }
        }

        public static XElement GetChildElementByAttrValue(this XElement source, string name, string attr, string attrValue)
        {
            var elements = source.Elements()
                .Where(e => String.Compare(e.Name.LocalName, name, StringComparison.OrdinalIgnoreCase) == 0);

            return elements
                .FirstOrDefault(e => e.Attributes().Any(a => (a.Name == attr.GetNameWithNamespace(source, String.Empty)) && (a.Value == attrValue)));

        }

        public static XElement CreateChildElement(this XElement source, string documentNamespace, string elementName,
            string attrName = null, string attrValue = null)
        {
            var item = new XElement(elementName.GetNameWithNamespace(source, documentNamespace));

            if (!String.IsNullOrWhiteSpace(attrName) && !String.IsNullOrWhiteSpace(attrValue))
            {
                item.SetAttributeValue(attrName.GetNameWithNamespace(source, documentNamespace), attrValue);
            }

            source.Add(item);
            return item;
        }

        public static XElement FindChildByAttrFilterMatch(this XElement source, Match attributeFilterMatch,
            string documentNamespace)
        {
            var elementName = attributeFilterMatch.Groups["element"].Value;
            var attrName = attributeFilterMatch.Groups["key"].Value;
            var attrValue = attributeFilterMatch.Groups["value"].Value;

            var item = source?.GetChildElementByAttrValue(elementName, attrName, attrValue);
            return item ?? source.CreateChildElement(documentNamespace, elementName, attrName, attrValue);
        }
    }
}