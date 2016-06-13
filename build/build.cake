#addin "Cake.Powershell"
#addin "Newtonsoft.Json"

// Helpers

Func<string, string> resolveFilePath = (string source) => MakeAbsolute(File(source)).ToString();
Func<string, string> resolveDirectoryPath = (string source) => MakeAbsolute(Directory(source)).ToString();

// Default target

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var version = Argument("versionnumber", "1.0.0");
var versionbuild = Argument("versionbuild", "0");

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
        CopyDirectory(resolveDirectoryPath(paths.root + "/tools"), resolveDirectoryPath(paths.workingDirSources + "/tools"));
        CopyDirectory(resolveDirectoryPath(paths.root + "/nuspecs"), resolveDirectoryPath(paths.workingDirSources + "/nuspecs"));
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
            XmlPoke(path, "/nuspec:package/nuspec:metadata/nuspec:version", version, new XmlPokeSettings {
                Namespaces = new Dictionary<string, string> {
                    { "nuspec", "http://schemas.microsoft.com/packaging/2011/10/nuspec.xsd" }
                }
            });
        }

        // Update VSTS manifest version
        var vssFiles = GetFiles(paths.workingDirSolutionDir + "/MagicChunks/**/vss-extension.json");
        foreach (var f in vssFiles)
        {
            var o = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(System.IO.File.ReadAllText(f.FullPath));
            o["version"] = version;
            System.IO.File.WriteAllText(f.FullPath, Newtonsoft.Json.JsonConvert.SerializeObject(o));
        }

    });

Task("Build")
    .Description("Builds sources")
    .IsDependentOn("PrepareOutputFolders")
    .IsDependentOn("UpdateVersion")
    .Does(() => {
        MSBuild(paths.workingDirSolutionPath, new MSBuildSettings {
            Verbosity = Verbosity.Minimal,
            ToolVersion = MSBuildToolVersion.VS2015,
            Configuration = configuration,
            PlatformTarget = PlatformTarget.MSIL,
        }.WithTarget("Build"));

        CopyFiles(paths.workingDirSolutionDir + "/MagicChunks" + "/bin/" + configuration + "/MagicChunks*.dll", paths.workingDirDotNet);
        CopyFiles(paths.workingDirSolutionDir + "/MagicChunks.Cake" + "/bin/" + configuration + "/MagicChunks*.dll", paths.workingDirDotNet);
        CopyFiles(paths.workingDirSolutionDir + "/MagicChunks/MSBuild/*.targets", paths.workingDirDotNet);
        CopyFiles(paths.workingDirSolutionDir + "/MagicChunks/Powershell/*.ps*", paths.workingDirDotNet);
    });

Task("Pack")
    .Description("Packs library into different packages")
    .IsDependentOn("PackNuget")
    .IsDependentOn("PackVSTS");

Task("PackNuget")
    .Description("Packs library into Nuget package")
    .IsDependentOn("Build")
    .Does(() => {
        var lib = paths.workingDirNuget + "/Net45/MagicChunks";
        EnsureDirectoryExists(lib);
        
        CopyFiles(resolveDirectoryPath(paths.workingDirSources + "/nuspecs") + "/*.nuspec", paths.workingDirNuget);
        CopyFiles(paths.workingDirDotNet + "/**/*.*", paths.workingDirNuget + "/Net45/MagicChunks");

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

        StartPowershellFile(paths.workingDirVSTS + "/_build.ps1", new PowershellSettings {
            WorkingDirectory = paths.workingDirVSTS
        }.SetFormatOutput());
    });


Task("Default")
    .IsDependentOn("Build")
    .IsDependentOn("Pack");

// Run

RunTarget(target);