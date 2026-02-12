using CsProj.Core;
using CsProj.Domain;

using NuGet.Versioning;

using Spectre.Console;

namespace CsProj.Commands.Modify;

internal sealed class DontUseCentralPackageManagementCommand : BaseModifyProjectsCommand<DontUseCentralPackageManagementCommand.Settings>
{
    public class Settings : BaseModifySettings
    {
    }

    public DontUseCentralPackageManagementCommand(ILogger logger, IAnsiConsole console, TimeProvider timeProvider)
        : base(logger, console, timeProvider)
    {
    }

    protected override bool TryModifyProjects(IEnumerable<CsharpProject> sdkprojects, Settings settings)
    {

        if (!CentralPackageReferences.TryGetCpmFilePath(settings.Path, out string? cpmFilePath))
        {
            _logger.Error("Central package management file path could not be determined.");
            return false;
        }

        if (!File.Exists(cpmFilePath))
        {
            _logger.Error("Central package management file '{0}' does not exist.", cpmFilePath);
            return false;
        }

        _logger.Info("Reading central package management file from '{0}'.", cpmFilePath);
        var xmlContent = File.ReadAllText(cpmFilePath);
        var centralPackages = CentralPackageReferences.ParseFromXml(xmlContent);

        foreach (var project in sdkprojects)
        {
            project.SetManagePackageVersionsCentrally(false);
            var packagesInProject = project.GetPackageReferences().Select(x => x.PackageName);
            foreach (var packageName in packagesInProject)
            {
                if (centralPackages.TryGetValue(packageName, out NuGetVersion? version))
                {
                    project.SetPackageReference(packageName, version.ToString());
                }
            }
        }

        _logger.Info("Deactivating central package management file '{0}'.", cpmFilePath);
        File.Move(cpmFilePath, Path.ChangeExtension(cpmFilePath, ".disabled"), true);

        return true;
    }
}

internal sealed class UseCentralPackageManagementCommand : BaseModifyProjectsCommand<UseCentralPackageManagementCommand.Settings>
{
    public class Settings : BaseModifySettings
    {
    }

    public UseCentralPackageManagementCommand(ILogger logger, IAnsiConsole console, TimeProvider timeProvider)
        : base(logger, console, timeProvider)
    {
    }

    protected override bool TryModifyProjects(IEnumerable<CsharpProject> sdkprojects, Settings settings)
    {
        Dictionary<string, NuGetVersion> collectedVersions = new();

        foreach (var project in sdkprojects)
        {
            foreach (var package in project.GetPackageReferences())
            {
                if (package.Version is null)
                    continue;

                if (collectedVersions.TryGetValue(package.PackageName, out NuGetVersion? value)
                    && package.Version > value)
                {
                    collectedVersions[package.PackageName] = package.Version;
                }
                else
                {
                    collectedVersions.Add(package.PackageName, package.Version);
                }

                project.RemovePackageReferenceVersion(package.PackageName);
                project.SetManagePackageVersionsCentrally(true);
            }
        }

        if (!CentralPackageReferences.TryGetCpmFilePath(settings.Path, out string? cpmFilePath))
        {
            _logger.Error("Central package management file path could not be determined.");
            return false;
        }

        _logger.Info("Writing central package management file to '{0}'.", cpmFilePath);
        var xml =  CentralPackageReferences.ConvertToXml(collectedVersions);
        File.WriteAllText(cpmFilePath, xml);

        return true;
    }
}
