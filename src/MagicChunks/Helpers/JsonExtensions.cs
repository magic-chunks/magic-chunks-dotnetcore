using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace MagicChunks.Helpers
{
    public static class JsonExtensions
    {
        public static JProperty GetChildProperty(this JObject source, string name)
        {
            return source.Children()
                         .OfType<JProperty>()
                         .FirstOrDefault(e => String.Compare(e.Name, name, StringComparison.OrdinalIgnoreCase) == 0);
        }

        public static JToken GetChildPropertyValue(this JObject source, string name)
        {
            return source.Children()
                         .OfType<JProperty>()
                         .FirstOrDefault(e => String.Compare(e.Name, name, StringComparison.OrdinalIgnoreCase) == 0)?
                         .Value;
        }
    }
}