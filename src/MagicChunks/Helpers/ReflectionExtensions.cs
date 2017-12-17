using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MagicChunks.Helpers
{
    public static class ReflectionExtensions
    {
        public static IEnumerable<Type> GetAllTypes(this Assembly source, Func<TypeInfo, bool> predicate = null)
        {
            IEnumerable<TypeInfo> result;
            try
            {
                result = source.DefinedTypes.ToArray();
            }
            catch (ReflectionTypeLoadException ex)
            {
                result = ex.Types.Where(t => t != null).Select(t => t.GetTypeInfo()).ToArray();
            }

            if (predicate != null)
                result = result.Where(predicate);

            return result.Select(t => t.AsType()).ToArray();
        }
    }
}
