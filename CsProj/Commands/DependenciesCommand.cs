using System.ComponentModel;

using CsProj.Core;
using CsProj.Domain;

using Spectre.Console;
using Spectre.Console.Cli;

namespace CsProj.Commands;

internal class DependenciesCommand : BaseReadCommand<DependenciesCommand.Settings>
{
    public enum DependencyType
    {
        Project,
        Package
    }

    public enum OutputType
    {
        Console,
        Mermaid,
        Nomnoml
    }

    public sealed class Settings : BaseReadSettings
    {
        [Description("Dependency type to visualize. Can be project or package")]
        [CommandOption(CommandOptions.DependencyType)]
        public DependencyType DependencyType { get; init; } = DependencyType.Project;

        [Description("Output type, can be console, mermaid or nomnoml")]
        [CommandOption(CommandOptions.OutputType)]
        public OutputType OutputType { get; init; } = OutputType.Console;
    }

    public DependenciesCommand(ILogger logger, IAnsiConsole console, TimeProvider timeProvider)
        : base(logger, console, timeProvider)
    {
    }

    protected override void CollectDataFromProjects(IEnumerable<IReadonlyCsharpProject> sdkprojects, Settings settings, CancellationToken cancellationToken)
    {
        DependencyTree dependencies = settings.DependencyType switch
        {
            DependencyType.Project => DependencyTree.CreateProjectTree(sdkprojects),
            DependencyType.Package => DependencyTree.CreatePackageReferenceTree(sdkprojects),
            _ => throw new NotSupportedException($"Dependency type '{settings.DependencyType}' is not supported.")
        };

        switch (settings.OutputType)
        {
            case OutputType.Console:
                _console.Tree(dependencies);
                break;
            case OutputType.Mermaid:
                _console.WriteLine(dependencies.ToMermaid());
                break;
            case OutputType.Nomnoml:
                _console.WriteLine(dependencies.ToNomnoml());
                break;
            default:
                throw new NotSupportedException($"Output format '{settings.OutputType}' is not supported.");
        }
    }
}
