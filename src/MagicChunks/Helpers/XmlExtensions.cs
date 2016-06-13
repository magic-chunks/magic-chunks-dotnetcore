using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace MagicChunks.Helpers
{
    public static class XmlExtensions
    {
        public static XElement GetChildElementByName(this XElement source, string name)
        {
            return source?.Elements()
                .FirstOrDefault(e => String.Compare(e.Name.LocalName, name, StringComparison.InvariantCultureIgnoreCase) == 0);
        }

        public static XElement GetChildElementByAttrValue(this XElement source, string name, string attr, string attrValue)
        {
            return source.Elements()
                .Where(e => String.Compare(e.Name.LocalName, name, StringComparison.InvariantCultureIgnoreCase) == 0)
                .FirstOrDefault(e => e.Attributes()
                    .Any(a => (String.Compare(a.Name.LocalName, attr, StringComparison.InvariantCultureIgnoreCase) == 0) && (a.Value == attrValue))
                );

        }

        public static XElement CreateChildElement(this XElement source, string documentNamespace, string elementName,
            string attrName = null, string attrValue = null)
        {
            var item = new XElement(XName.Get(elementName, documentNamespace));

            if (!String.IsNullOrWhiteSpace(attrName) && !String.IsNullOrWhiteSpace(attrValue))
                item.SetAttributeValue(XName.Get(attrName, documentNamespace), attrValue);

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