#:sdk Cake.Sdk@6.1.1

var target = Argument("target", "Test");
var configuration = Argument("configuration", "Release");

const string TestProject = "./BeyondTrust.SecretSafeProvider.Tests/BeyondTrust.SecretSafeProvider.Tests.csproj";
const string CoverageReport = "./coverage-report";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Build")
    .Does(() =>
{
    DotNetBuild("./BeyondTrust.SecretSafeProvider", new DotNetBuildSettings
    {
        Configuration = configuration,
    });
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    var coverageOutput = MakeAbsolute(File("./TestResults/coverage.cobertura.xml")).FullPath;

    DotNetRun(TestProject, new DotNetRunSettings
    {
        Configuration = configuration,
        ArgumentCustomization = args => args
            .Append("--coverage")
            .Append("--coverage-output-format").Append("cobertura")
            .Append("--coverage-output").Append(coverageOutput)
    });
});

Task("Coverage")
    .IsDependentOn("Test")
    .Does(() =>
{
    var coverageOutput = MakeAbsolute(File("./TestResults/coverage.cobertura.xml")).FullPath;
    var coverageReport = MakeAbsolute(Directory(CoverageReport)).FullPath;

    DotNetTool("reportgenerator", new DotNetToolSettings
    {
        ArgumentCustomization = args => args
            .Append($"-reports:{coverageOutput}")
            .Append($"-targetdir:{coverageReport}")
            .Append("-reporttypes:Html;Badges")
            .Append("-assemblyfilters:+terraform-provider-beyondtrust-secretsafe")
            .Append("-classfilters:-BeyondTrust.SecretSafeProvider.Proto.*")
    });

    Information($"Coverage report generated at: {coverageReport}/index.html");
});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
