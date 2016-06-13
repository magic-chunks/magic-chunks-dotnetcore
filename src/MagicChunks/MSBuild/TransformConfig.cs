using System;
using MagicChunks.Core;
using Microsoft.Build.Framework;

namespace MagicChunks.MSBuild
{
    public class TransformConfig : ITask
    {
        [Required]
        public string Path { get; set; }

        public string Target { get; set; }

        public string Type { get; set; }

        public ITaskItem[] Trasformations { get; set; }

        public IBuildEngine BuildEngine { get; set; }

        public ITaskHost HostObject { get; set; }

        public bool Execute()
        {
            bool result = true;

            BuildEngine.LogMessageEvent(new BuildMessageEventArgs($"Transofrming file: {Path}", string.Empty,
                nameof(TransformConfig), MessageImportance.Normal));

            try
            {
                TransformationCollection transforms = new TransformationCollection();

                foreach (var item in Trasformations)
                {
                    if (transforms.ContainsKey(item.ItemSpec) == false)
                        transforms.Add(item.ItemSpec, item.GetMetadata("Value"));
                    else
                    {
                        BuildEngine.LogWarningEvent(new BuildWarningEventArgs(
                            null,
                            null,
                            null,
                            0, 0, 0, 0,
                            $"Transform key duplicate: {item.ItemSpec}. Skipping.", null,
                            nameof(TransformConfig), DateTime.Now));
                        result = false;
                    }
                }

                TransformTask.Transform(Type, Path, Target ?? Path, transforms);

                BuildEngine.LogMessageEvent(new BuildMessageEventArgs($"File transformed to: {Target ?? Path}", string.Empty,
                    nameof(TransformConfig), MessageImportance.Normal));
            }
            catch (Exception ex)
            {
                BuildEngine.LogErrorEvent(new BuildErrorEventArgs(
                    null,
                    null,
                    null,
                    0, 0, 0, 0,
                    $"File transformation error: {ex.Message}", null,
                    nameof(TransformConfig), DateTime.Now));

                result = false;
            }

            return result;
        }
    }
}