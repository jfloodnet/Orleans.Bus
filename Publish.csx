using Nake;
using Nake.FS;
using Nake.Run;

using System.Diagnostics;
using System.Dynamic;

const string Project = "Orleans.Bus";

var OutputPath = @"$NakeScriptDirectory$\Output";
var PackagePath = @"{OutputPath}\Package";

var DebugOutputPath = @"{PackagePath}\Debug";
var ReleaseOutputPath = @"{PackagePath}\Release";

Func<string> PackageFile = () => PackagePath + @"\{Project}.{Version()}.nupkg";
Func<string> ReactivePackageFile = () => PackagePath + @"\{Project}.Reactive.{Version()}.nupkg";
Func<string> TestingPackageFile = () => PackagePath + @"\{Project}.Testing.{Version()}.nupkg";

/// Publishes package to NuGet gallery
[Step] void NuGet()
{
    Cmd(@"Tools\Nuget.exe push {PackageFile()} $NuGetApiKey$");
    Cmd(@"Tools\Nuget.exe push {ReactivePackageFile()} $NuGetApiKey$");
    Cmd(@"Tools\Nuget.exe push {TestingPackageFile()} $NuGetApiKey$");
}

string Version()
{
    return FileVersionInfo
           	.GetVersionInfo(@"{ReleaseOutputPath}\{Project}.dll")
           	.ProductVersion;
}