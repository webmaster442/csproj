using CsProj.Core;
using CsProj.Domain;

using NuGet.Versioning;

using Spectre.Console;

namespace CsProj.Commands.Modify;

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

        string directory = GetCpmDirectory(settings.Path);

        if (!Directory.Exists(directory))
        {
            _logger.Error("The directory '{0}' does not exist.", directory);
            return false;
        }

        var propsFile = Path.Combine(directory, "Directory.Packages.props");

        _logger.Info("Writing central package management file to '{0}'.", propsFile);
        var xml =  CentralPackageReferences.ConvertToXml(collectedVersions);
        File.WriteAllText(propsFile, xml);

        return true;
    }

    private static string GetCpmDirectory(string path)
    {
        if (File.Exists(path))
        {
            // It's a file, return its directory
            return Path.GetDirectoryName(path)!;
        }
        // It's a directory, return it as is
        return path;
    }
}
