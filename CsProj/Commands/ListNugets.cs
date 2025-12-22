using CsProj.Core;
using CsProj.Domain;

using Spectre.Console;

namespace CsProj.Commands;

internal sealed class ListNugets : BaseReadCommand<BaseReadSettings>
{
    public ListNugets(ILogger logger, IAnsiConsole console, TimeProvider timeProvider)
        : base(logger, console, timeProvider)
    {
    }

    protected override void CollectDataFromProjects(IEnumerable<IReadonlyCsharpProject> sdkprojects,
                                                    BaseReadSettings settings,
                                                    CancellationToken cancellationToken)
    {
        HashSet<PackageReference> packageReferences = new();

        foreach (var project in sdkprojects)
        {
            foreach (var packageReference in project.GetPackageReferences())
            {
                packageReferences.Add(packageReference);
            }
        }

        _console.Table(packageReferences.OrderBy(x => x.PackageName));
    }
}
