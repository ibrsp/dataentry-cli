//////////////////////////////////////////////////////////////////////
// TOOLS
//////////////////////////////////////////////////////////////////////
#tool "nuget:?package=GitVersion.CommandLine"
#tool "nuget:?package=gitreleasemanager"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////
var target = Argument("target", "default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Should MSBuild treat any errors as warnings?
var treatWarningsAsErrors = "false";

// Build configuration
var local = BuildSystem.IsLocalBuild;
var isRunningOnUnix = IsRunningOnUnix();
var isRunningOnWindows = IsRunningOnWindows();

var isRunningOnAppVeyor = AppVeyor.IsRunningOnAppVeyor;
var isPullRequest = AppVeyor.Environment.PullRequest.IsPullRequest;
var isReleaseBranch = StringComparer.OrdinalIgnoreCase.Equals("master", AppVeyor.Environment.Repository.Branch);
var isTagged = AppVeyor.Environment.Repository.Tag.IsTag;

// Artifacts
var artifactDirectory = "./artifacts/";

// Version
var gitVersion = GitVersion();
var majorMinorPatch = gitVersion.MajorMinorPatch;
var semVersion = gitVersion.SemVer;
var informationalVersion = gitVersion.InformationalVersion;
var buildVersion = gitVersion.FullBuildMetaData;

// Project/Solution specific files
var solutionFile = "DataEntry.Cli.sln";
var assemblyInfo = "./src/DataEntry.Cli/Properties/AssemblyInfo.cs"; 

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////
Setup((context) =>
{
    Information("Building version {0} of fusillade. (isTagged: {1})", informationalVersion, isTagged);
});

Teardown((context) =>
{
    // Executed AFTER the last task.
});

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////
Task("clean")
    .Does(() =>
{
    CleanDirectories(artifactDirectory);
    CleanDirectories("./src/**/obj");
});

Task("nuget-restore")
    .Does(() => 
{
    NuGetRestore(solutionFile);
});

Task("update-assembly-info")
    .WithCriteria(() => local == false)
    .Does(() => 
{
    CreateAssemblyInfo(assemblyInfo, new AssemblyInfoSettings {
        Product = "DataEntry.Cli",
        Version = majorMinorPatch,
        FileVersion = majorMinorPatch,
        InformationalVersion = informationalVersion,
        Copyright = "Copyright (c) Eugene Sergueev"
    });
});

Task("update-appveyor-build-number")
    .WithCriteria(() => isRunningOnAppVeyor)
    .Does(() => AppVeyor.UpdateBuildVersion(buildVersion));

Task("update-version")
    .IsDependentOn("update-assembly-info")
    .IsDependentOn("update-appveyor-build-number");

Task("build")
    .IsDependentOn("clean")
    .IsDependentOn("nuget-restore")
    .IsDependentOn("update-version")
    .Does(() =>
{
    DotNetBuild(solutionFile, settings =>
                settings.SetConfiguration(configuration)
                        .SetVerbosity(Verbosity.Minimal)
                        .WithTarget("Build")
                        .WithProperty("TreatWarningsAsErrors", treatWarningsAsErrors)); 
    
    var buildArtifactsPath = GetDirectories("./src/**/bin/" + configuration).Single();
    var outputFileName =  string.Concat(artifactDirectory, "v", semVersion, ".zip");
    EnsureDirectoryExists(artifactDirectory);
    Zip(buildArtifactsPath, outputFileName);
   

});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////
Task("default")
  .IsDependentOn("build");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////
RunTarget(target);