using System;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace MagicChunks.Helpers
{
    public static class JsonExtensions
    {
        private static readonly Regex NodeIndexEndingRegex = new Regex(@"\[\d+\]$", RegexOptions.CultureInvariant | RegexOptions.Compiled);
        private static readonly Regex NodeArrayEndingRegex = new Regex(@"\[\]$", RegexOptions.CultureInvariant | RegexOptions.Compiled);
        private static readonly Regex NodeValueEndingRegex = new Regex(@"\[\@.+\=.+\]$", RegexOptions.CultureInvariant | RegexOptions.Compiled);
        private static readonly Regex NodeValueRegex = new Regex(@"^\@(.+)\=(.+)$", RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public static JToken GetChildProperty(this JContainer source, string name)
        {
            if (NodeIndexEndingRegex.IsMatch(name))
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

                var elements = source.Children()
                    .OfType<JProperty>()
                    .FirstOrDefault(e => String.Compare(e.Name, nodeName, StringComparison.OrdinalIgnoreCase) == 0)?
                    .Children()
                    .FirstOrDefault();

                return elements.Skip(nodeIndex).FirstOrDefault();
            }
            else if (NodeValueEndingRegex.IsMatch(name))
            {
                string nodeName = NodeValueEndingRegex.Replace(name, String.Empty);
                var nodeSelector = ParseNodeValueFilter(NodeValueEndingRegex.Match(name).Value);

                var elements = source.Children()
                    .OfType<JProperty>()
                    .FirstOrDefault(e => String.Compare(e.Name, nodeName, StringComparison.OrdinalIgnoreCase) == 0)?
                    .Children()
                    .FirstOrDefault()
                    .OfType<JObject>();

                return elements.FirstOrDefault(e =>
                {
                    if ((e.GetChildProperty(nodeSelector.paramName) as JProperty)?.Value is JValue)
                    {
                        return (e.GetChildProperty(nodeSelector.paramName) as JProperty)?.Value.ToString() == nodeSelector.paramValue;
                    }
                    else
                    {
                        return false;
                    }
                });
            }
            else
            {
                return source.Children()
                         .OfType<JProperty>()
                         .FirstOrDefault(e => String.Compare(e.Name, name, StringComparison.OrdinalIgnoreCase) == 0);
            }
        }

        public static JToken GetChildPropertyValue(this JContainer source, string name)
        {
            if (NodeIndexEndingRegex.IsMatch(name))
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

                var elements = source.Children()
                    .OfType<JProperty>()
                    .FirstOrDefault(e => String.Compare(e.Name, nodeName, StringComparison.OrdinalIgnoreCase) == 0)?
                    .Children()
                    .FirstOrDefault();

                return elements.Skip(nodeIndex).FirstOrDefault();
            }
            else if (NodeArrayEndingRegex.IsMatch(name))
            {
                string nodeName = NodeArrayEndingRegex.Replace(name, String.Empty);

                return source.Children()
                    .OfType<JProperty>()
                    .FirstOrDefault(e => String.Compare(e.Name, nodeName, StringComparison.OrdinalIgnoreCase) == 0)?
                    .Children()
                    .FirstOrDefault();
            }
            else if (NodeValueEndingRegex.IsMatch(name))
            {
                string nodeName = NodeValueEndingRegex.Replace(name, String.Empty);
                var nodeSelector = ParseNodeValueFilter(NodeValueEndingRegex.Match(name).Value);

                var elements = source.Children()
                    .OfType<JProperty>()
                    .FirstOrDefault(e => String.Compare(e.Name, nodeName, StringComparison.OrdinalIgnoreCase) == 0)?
                    .Children()
                    .FirstOrDefault()
                    .OfType<JObject>();

                return elements.FirstOrDefault(e =>
                {
                    if ((e.GetChildProperty(nodeSelector.paramName) as JProperty)?.Value is JValue)
                    {
                        return (e.GetChildProperty(nodeSelector.paramName) as JProperty)?.Value.ToString() == nodeSelector.paramValue;
                    }
                    else
                    {
                        return false;
                    }
                });
            }
            else
            {
                return source.Children()
                         .OfType<JProperty>()
                         .FirstOrDefault(e => String.Compare(e.Name, name, StringComparison.OrdinalIgnoreCase) == 0)?
                         .Value;
            }
        }

        public static (string paramName, string paramValue) ParseNodeValueFilter(this string filter)
        {
            var match = NodeValueRegex.Match(filter.Trim(' ', '[', ']'));
            if (match.Success && (match.Groups.Count == 3))
            {
                return (match.Groups[1].Value, match.Groups[2].Value.Trim('\"', '\''));
            }
            else
            {
                throw new ArgumentException($"Wrong expression: {filter}", nameof(filter));
            }
        }
    }
}