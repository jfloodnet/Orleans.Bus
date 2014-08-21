#r "System.Xml"
#r "System.Xml.Linq"

using Nake.FS;
using Nake.Run;
using Nake.Log;

using System.Linq;
using System.Xml.Linq;
using System.Diagnostics;

const string MainProject = "Orleans.Bus";
const string TestingProject = "Orleans.Bus.Testing";
const string ReactiveProject = "Orleans.Bus.Reactive";

const string RootPath = "$NakeScriptDirectory$";
const string OutputPath = RootPath + @"\Output";

var PackagePath = @"{OutputPath}\Package";
var ReleasePath = @"{PackagePath}\Release";

/// Builds sources in Debug mode
[Task] void Default()
{
	Build();
}

/// Wipeout all build output and temporary build files
[Step] void Clean(string path = OutputPath)
{
    Delete(@"{path}\*.*|-:*.vshost.exe");
    RemoveDir(@"**\bin|**\obj|{path}\*|-:*.vshost.exe");
}

/// Builds sources using specified configuration and output path
[Step] void Build(string config = "Debug", string outDir = OutputPath)
{
    Clean(outDir);

    MSBuild("{MainProject}.sln", 
            "Configuration={config};OutDir={outDir};ReferencePath={outDir}");
}

/// Runs unit tests 
[Step] void Test(string outputPath = OutputPath)
{
    Build("Debug", outputPath);

    FileSet tests = @"{outputPath}\*.Tests.dll";
    Cmd(@"Packages\NUnit.Runners.2.6.3\tools\nunit-console.exe /framework:net-4.0 /noshadow /nologo {tests}");
}

/// Builds official NuGet packages 
[Step] void Package()
{
    Test(@"{PackagePath}\Debug");
    Build("Release", ReleasePath);

    Pack(MainProject);
    Pack(ReactiveProject, "bus_version={Version(MainProject)}");
    Pack(TestingProject,  "bus_version={Version(MainProject)}");
}

void Pack(string project, string properties = null)
{
    Cmd(@"Tools\Nuget.exe pack Build\{project}.nuspec -Version {Version(project)} " +
         "-OutputDirectory {PackagePath} -BasePath {RootPath} -NoPackageAnalysis " + 
         (properties != null ? "-Properties {properties}" : ""));
}

/// Publishes package to NuGet gallery
[Step] void Publish(string project)
{
    switch (project)
    {
        case "bus": 
            Push(MainProject); 
            break;
        case "reactive": 
            Push(ReactiveProject); 
            break;
        case "testing": 
            Push(TestingProject); 
            break;
        default:
            throw new ArgumentException("Available values are: bus, reactive or testing");   
    }
}

void Push(string project)
{
    Cmd(@"Tools\Nuget.exe push {PackagePath}\{project}.{Version(project)}.nupkg $NuGetApiKey$");
}

string Version(string project)
{
    return FileVersionInfo
            .GetVersionInfo(@"{ReleasePath}\{project}.dll")
            .FileVersion;
}