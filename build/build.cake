#addin nuget:?package=Cake.Git&version=0.22.0
#addin nuget:?package=Cake.Npm&version=0.17.0
#addin nuget:?package=Cake.Tfx&version=0.9.1

#addin nuget:?package=Newtonsoft.Json&version=12.0.3

#tool nuget:?package=ILRepack&version=2.0.18

using System.Text.RegularExpressions;
using System.Linq;

// Helpers

Func<string, string> resolveFilePath = (string source) => MakeAbsolute(File(source)).ToString();
Func<string, string> resolveDirectoryPath = (string source) => MakeAbsolute(Directory(source)).ToString();

// Default target

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var versionbuild = EnvironmentVariable("appveyor_build_version", "0");

var tags = GitTags(resolveDirectoryPath("./../"));
var lastTag = tags.Last();
var matches = Regex.Matches(lastTag.ToString(), "v([0-9\\.]+)", RegexOptions.IgnoreCase);
var version = "0.0.0";
if (matches[0].Success && matches[0].Groups.Count > 1)
{
    version = matches[0].Groups[1].Value;
}

// Variables

var paths = new {
    root = resolveDirectoryPath("./../"),

    workingDir = resolveDirectoryPath("./../working/"),
    workingDirSources = resolveDirectoryPath("./../working/sources/"),
    workingDirDotNet = resolveDirectoryPath("./../working/dotnet/"),
    workingDirNuget = resolveDirectoryPath("./../working/nuget/"),
    workingDirVSTS = resolveDirectoryPath("./../working/vsts/"),
    workingDirSolutionDir = resolveDirectoryPath("./../working/sources/src/"),
    workingDirSolutionPath = resolveDirectoryPath("./../working/sources/src/MagicChunks.sln"),

    solutionDir = resolveDirectoryPath("./../src/"),
    solutionPath = resolveDirectoryPath("./../src/MagicChunks.sln"),
};

// Tasks

Task("PrepareOutputFolders")
    .Description("Creates folder for project build output")
    .Does(() => {
        EnsureDirectoryExists(paths.workingDir);
        CleanDirectories(paths.workingDir);

        EnsureDirectoryExists(paths.workingDirSources);
        EnsureDirectoryExists(paths.workingDirDotNet);
        EnsureDirectoryExists(paths.workingDirNuget);
        EnsureDirectoryExists(paths.workingDirVSTS);

        CopyDirectory(resolveDirectoryPath(paths.root + "/src"), resolveDirectoryPath(paths.workingDirSources + "/src"));
        CopyDirectory(resolveDirectoryPath(paths.root + "/nuspecs"), resolveDirectoryPath(paths.workingDirSources + "/nuspecs"));
    });

Task("RestorePackages")
    .Description("Restore Nuget packages for the project")
    .IsDependentOn("PrepareOutputFolders")
    .Does(() => {
        NuGetRestore(paths.workingDirSolutionPath);
    });


Task("UpdateVersion")
    .Description("Updates version for assembly and packages")
    .Does(() => {

        // Update assembly version

        var assemblyInfoFiles = GetFiles(paths.workingDirSources + "/**/AssemblyInfo.cs");
        foreach (var path in assemblyInfoFiles)
        {
            var assemblyInfo = ParseAssemblyInfo(path);

            CreateAssemblyInfo(path, new AssemblyInfoSettings {
                Title = assemblyInfo.Title,
                Product = assemblyInfo.Product,
                Company = assemblyInfo.Company,
                ComVisible = assemblyInfo.ComVisible,
                Configuration = assemblyInfo.Configuration,
                Description = assemblyInfo.Description,
                Guid = assemblyInfo.Guid,
                InternalsVisibleTo = assemblyInfo.InternalsVisibleTo,
                Version = string.Format("{0}.{1}", version, versionbuild),
                FileVersion = string.Format("{0}.{1}", version, versionbuild),
                InformationalVersion = string.Format("{0}-{1}", version, versionbuild),
                Copyright = assemblyInfo.Copyright,
                Trademark = assemblyInfo.Trademark
            });
        }

        // Update nuspecs version

        var nuspecFiles = GetFiles(paths.workingDirSources + "/**/*.nuspec");
        foreach (var path in nuspecFiles)
        {
            Information("Nuspec: " + path);
            XmlPoke(path, "/nuspec:package/nuspec:metadata/nuspec:version", version, new XmlPokeSettings {
                Namespaces = new Dictionary<string, string> {
                    { "nuspec", "http://schemas.microsoft.com/packaging/2012/06/nuspec.xsd" }
                }
            });
        }

        // Update VSTS manifest version
        var vssFiles = GetFiles(paths.workingDirSolutionDir + "/MagicChunks/**/vss-extension.json");
        foreach (var f in vssFiles)
        {
            var o = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(System.IO.File.ReadAllText(f.FullPath));
            o["version"] = version;
            System.IO.File.WriteAllText(f.FullPath, Newtonsoft.Json.JsonConvert.SerializeObject(o, Newtonsoft.Json.Formatting.Indented));
        }

        var versionMatch = System.Text.RegularExpressions.Regex.Match(version, @"^(\d+)\.(\d+)\.(\d+)$");
        if (versionMatch.Success && (versionMatch.Groups.Count == 4))
        {
            var taskFiles = GetFiles(paths.workingDirSolutionDir + "/MagicChunks/**/task.json");
            foreach (var f in taskFiles)
            {
                var o = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(System.IO.File.ReadAllText(f.FullPath));
                if ((o["version"] != null) && (o["version"]["Major"] != null) && (o["version"]["Minor"] != null) && (o["version"]["Patch"] != null))
                {
                    o["version"]["Major"] = int.Parse(versionMatch.Groups[1].Value);
                    o["version"]["Minor"] = int.Parse(versionMatch.Groups[2].Value);
                    o["version"]["Patch"] = int.Parse(versionMatch.Groups[3].Value);
                    System.IO.File.WriteAllText(f.FullPath, Newtonsoft.Json.JsonConvert.SerializeObject(o, Newtonsoft.Json.Formatting.Indented));
                }
                else
                {
                    Warning("Manifest " + f + " does not contains version property");
                }
            }
        }
        else
        {
            Warning("Unable to parse version: " + version);
        }
    });

Task("Build")
    .Description("Builds sources")
    .IsDependentOn("PrepareOutputFolders")
    .IsDependentOn("RestorePackages")
    .IsDependentOn("UpdateVersion")
    .Does(() => {
        MSBuild(paths.workingDirSolutionPath, new MSBuildSettings {
            Verbosity = Verbosity.Minimal,
            ToolVersion = MSBuildToolVersion.VS2019,
            Configuration = configuration,
            PlatformTarget = PlatformTarget.MSIL,
        }.WithTarget("Build"));

        ILRepack(
            paths.workingDirSolutionDir + "/MagicChunks/bin/" + configuration + "/netstandard1.3/MagicChunks.dll",
            paths.workingDirSolutionDir + "/MagicChunks/bin/" + configuration + "/netstandard1.3/MagicChunks.dll",
            GetFiles(paths.workingDirSolutionDir + "/MagicChunks/bin/" + configuration + "/netstandard1.3/*.dll"),
            new ILRepackSettings { Internalize = true });

        CopyFiles(paths.workingDirSolutionDir + "/MagicChunks" + "/bin/" + configuration + "/netstandard1.3/MagicChunks*.dll", paths.workingDirDotNet);
        CopyFiles(paths.workingDirSolutionDir + "/MagicChunks.Cake" + "/bin/" + configuration + "/netstandard1.6/MagicChunks.Cake.dll", paths.workingDirDotNet);
        CopyFiles(paths.workingDirSolutionDir + "/MagicChunks.Cake" + "/bin/" + configuration + "/netstandard2.0/MagicChunks.Cake.dll", paths.workingDirDotNet);
        CopyFiles(paths.workingDirSolutionDir + "/MagicChunks/MSBuild/*.targets", paths.workingDirDotNet);
        CopyFiles(paths.workingDirSolutionDir + "/MagicChunks/Powershell/*.ps*", paths.workingDirDotNet);
    });

Task("Pack")
    .Description("Packs library into different packages")
    .IsDependentOn("PackDotnet")
    .IsDependentOn("PackNuget")
    .IsDependentOn("PackVSTS");

Task("PackDotnet")
    .Description("Packs library into zip package")
    .IsDependentOn("Build")
    .Does(() => {
        Zip(paths.workingDirDotNet, System.IO.Path.Combine(paths.workingDirDotNet, "MagicChunks-" + version + ".zip"));
    });

Task("PackNuget")
    .Description("Packs library into Nuget package")
    .IsDependentOn("Build")
    .Does(() => {
        EnsureDirectoryExists(paths.workingDirNuget + "/netstandard1.3/MagicChunks");
        EnsureDirectoryExists(paths.workingDirNuget + "/netstandard1.6/MagicChunks");
        EnsureDirectoryExists(paths.workingDirNuget + "/netstandard2.0/MagicChunks");

        CopyFiles(resolveDirectoryPath(paths.workingDirSources + "/nuspecs") + "/*.nuspec", paths.workingDirNuget);
        CopyFiles(paths.workingDirDotNet + "/**/*.*", paths.workingDirNuget + "/netstandard1.3/MagicChunks");
        CopyFiles(paths.workingDirDotNet + "/**/*.*", paths.workingDirNuget + "/netstandard1.6/MagicChunks");
        CopyFiles(paths.workingDirDotNet + "/**/*.*", paths.workingDirNuget + "/netstandard2.0/MagicChunks");
        DeleteFile(paths.workingDirNuget + "/netstandard1.3/MagicChunks/MagicChunks.Cake.dll");

        foreach (string file in System.IO.Directory.EnumerateFiles(paths.workingDirNuget, "*.nuspec"))
        {
            NuGetPack(file, new NuGetPackSettings {
                Version = string.Format("{0}.{1}", version, versionbuild),
                OutputDirectory = paths.workingDirNuget
            });
        }
    });

Task("PackVSTS")
    .Description("Packs library into VSTS extension package")
    .IsDependentOn("Build")
    .Does(() => {

        CopyDirectory(paths.workingDirSolutionDir + "/MagicChunks/VSTS", paths.workingDirVSTS);
        CopyFiles(paths.workingDirDotNet + "/**/*.dll", paths.workingDirVSTS + "/MagicChunks");

        NpmInstall(settings => settings.AddPackage("npm").InstallGlobally().WithLogLevel(NpmLogLevel.Silent));
        NpmInstall(settings => settings.AddPackage("tfx-cli").InstallGlobally().WithLogLevel(NpmLogLevel.Silent));

        TfxExtensionCreate(new TfxExtensionCreateSettings()
        {
            WorkingDirectory = paths.workingDirVSTS,
            ManifestGlobs = new List<string>(){ paths.workingDirVSTS + "/vss-extension.json" }
        });
    });


Task("Default")
    .IsDependentOn("Build")
    .IsDependentOn("Pack");

// Run

Information("Version: " + string.Format("{0}.{1}", version, versionbuild));
Information("Version: " + string.Format("{0}-{1}", version, versionbuild));

RunTarget(target);