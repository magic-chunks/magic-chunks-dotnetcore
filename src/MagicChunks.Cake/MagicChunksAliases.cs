using System;
using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Diagnostics;
using MagicChunks.Core;

namespace MagicChunks.Cake
{
    /// <summary>
    /// Cake build aliases for MagicChunks
    /// </summary>
    [CakeAliasCategory("Configuration")]
    [CakeNamespaceImport("MagicChunks")]
    [CakeNamespaceImport("MagicChunks.Core")]
    public static class MagicChunksAliases
    {
        [CakeMethodAlias]
        public static bool TransformConfig(this ICakeContext context, string path,
            TransformationCollection trasformations)
        {
            return TransformConfig(context, null, path, path, trasformations);
        }

        [CakeMethodAlias]
        public static bool TransformConfig(this ICakeContext context, string path, string target,
            TransformationCollection trasformations)
        {
            return TransformConfig(context, null, path, target, trasformations);

        }

        [CakeMethodAlias]
        public static bool TransformConfig(this ICakeContext context, string type, string path, string target,
            TransformationCollection trasformations)
        {
            var result = true;

            context.Log.Write(Verbosity.Normal, LogLevel.Information, "Transofrming file: {0}", path);

            try
            {
                TransformTask.Transform(type, path, target ?? path, trasformations);

                context.Log.Write(Verbosity.Normal, LogLevel.Information, "File transformed to: {0}", target ?? path);
            }
            catch (Exception ex)
            {
                context.Log.Write(Verbosity.Normal, LogLevel.Error, "File transformation error: {0}", ex.Message);
                result = false;
            }

            return result;
        }
    }
}