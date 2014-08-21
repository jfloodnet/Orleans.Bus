#load Nake.csx

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