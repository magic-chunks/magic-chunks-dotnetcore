using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace MagicChunks.Helpers
{
    public static class XmlExtensions
    {
        public static XName GetNameWithNamespace(this string name, XElement element, string defaultNamespace)
        {
            var preparedName = name.Split('[')[0];

            XName result;
            if (preparedName.Contains(':') == false)
                result = XName.Get(preparedName, defaultNamespace);
            else
            {
                var attributeNameParts = preparedName.Split(':');
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
            return source?.Elements()
                .FirstOrDefault(e => name.IndexOf(':') == -1 ? 
                                        String.Compare(e.Name.LocalName, name, StringComparison.InvariantCultureIgnoreCase) == 0 :
                                        e.Name == name.GetNameWithNamespace(source, String.Empty));
        }

        public static XElement GetChildElementByAttrValue(this XElement source, string name, string attr, string attrValue)
        {
            var elements = source.Elements()
                .Where(e => String.Compare(e.Name.LocalName, name, StringComparison.InvariantCultureIgnoreCase) == 0);

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