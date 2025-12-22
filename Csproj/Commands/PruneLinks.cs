using System.ComponentModel;

using Spectre.Console.Cli;
using Spectre.Console;
using Csproj.DomainServices;
using Csproj.Infrastructure;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace Csproj.Commands;

internal sealed class PruneLinks : Command<PruneLinks.Settings>
{
    private const string CsProj = ".csproj";
    
    public class Settings : CommandSettings
    {
        [Description("Solution file path (.sln or .slnx)")]
        [CommandOption("-s|--solution")]
        public string SolutionPath { get; set; } = string.Empty;

        [Description("Project file path (.csproj)")]
        [CommandOption("--csproj")]
        public string CsprojPath { get; set; } = string.Empty;

        [Description("Dryrun mode. Only show what would be changed.")]
        [CommandOption("-D|--dryrun")]
        public bool DryRun { get; set; }

        [Description("Create a backup of the project file before editing.")]
        [CommandOption("-b|--backup")]
        public bool Backup { get; set; }

        [Description("Show the reference tree for each project.")]
        [CommandOption("-v|--verbose")]
        public bool Verbose { get; set; }

        [Description("Output the dependency graph as a Markdown file with Mermaid syntax.")]
        [CommandOption("--graph-md")]
        public string GraphMdPath { get; set; } = string.Empty;
    }

    /// <summary>
    /// Executes the prune links command, validating input and performing project reference pruning.
    /// </summary>
    /// <param name="context">The command context.</param>
    /// <param name="settings">The command settings. See <see cref="Settings"/>.</param>
    /// <returns>Exit code: 0 for success, -1 for error.</returns>
    public override int Execute(CommandContext context, Settings settings)
    {
        var errorMessage = ValidateInput(settings);
        if (errorMessage is not null)
        {
            AnsiConsole.MarkupLine($"[red]{errorMessage}[/]");
            return -1;
        }

        (List<string> projects, string rootPath) = GetProjectsAndRootPath(settings);
        var allProjects = projects.ToHashSet();
        var originalGraph = ProjectManipulator.BuildDependencyGraph(projects);
        var changes = ProjectManipulator.PruneRedundantLinks(originalGraph, settings.DryRun, settings.Backup);
        var displayGraph = SelectDisplayGraph(settings, projects, originalGraph);

        if (settings.Verbose)
        {
            DisplayReferenceTrees(displayGraph, allProjects);
        }

        if (!string.IsNullOrWhiteSpace(settings.GraphMdPath))
        {
            OutputDependencyGraphMarkdown(settings, displayGraph, allProjects, rootPath);
        }

        ReportChanges(changes);
        return 0;
    }

    /// <summary>
    /// Selects the dependency graph to display based on dry run mode.
    /// </summary>
    /// <param name="settings">The command settings. See <see cref="Settings"/>.</param>
    /// <param name="projects">List of project paths.</param>
    /// <param name="originalGraph">The original dependency graph.</param>
    /// <returns>The dependency graph to display.</returns>
    private static Dictionary<string, List<string>> SelectDisplayGraph(
        Settings settings,
        List<string> projects,
        Dictionary<string, List<string>> originalGraph)
    {
        return settings.DryRun ? originalGraph : ProjectManipulator.BuildDependencyGraph(projects);
    }

    /// <summary>
    /// Reports the changes made to project references.
    /// </summary>
    /// <param name="changes">List of change descriptions.</param>
    private static void ReportChanges(List<string> changes)
    {
        if (changes.Count == 0)
        {
            AnsiConsole.MarkupLine("[green]No redundant links found.[/]");
            return;
        }

        foreach (var change in changes)
        {
            AnsiConsole.MarkupLine($"[yellow]{change}[/]");
        }
    }

    /// <summary>
    /// Gets the list of projects and the root path from the provided settings.
    /// </summary>
    /// <param name="settings">The command settings. See <see cref="Settings"/>.</param>
    /// <returns>Tuple containing the list of projects and the root path.</returns>
    private static (List<string> projects, string rootPath) GetProjectsAndRootPath(Settings settings)
    {
        bool hasSolution = !string.IsNullOrWhiteSpace(settings.SolutionPath);
        if (hasSolution)
        {
            var projects = SolutionFileParser.GetAllProjectPaths(settings.SolutionPath).ToList();
            return (projects, settings.SolutionPath);
        }
        else
        {
            var projects = ProjectManipulator.CollectAllReferencedProjects(settings.CsprojPath);
            return (projects, settings.CsprojPath);
        }
    }

    /// <summary>
    /// Validates the input settings for the command.
    /// </summary>
    /// <param name="settings">The command settings. See <see cref="Settings"/>.</param>
    /// <returns>Error message if invalid, otherwise null.</returns>
    private static string? ValidateInput(Settings settings)
    {
        bool hasSolution = !string.IsNullOrWhiteSpace(settings.SolutionPath);
        bool hasCsproj = !string.IsNullOrWhiteSpace(settings.CsprojPath);
        
        if (hasSolution == hasCsproj)
        {
            return ("You must specify either --solution or --csproj, but not both.");
        }
        
        if (hasSolution)
        {
            if (!File.Exists(settings.SolutionPath))
            {
                return ($"Solution file not found: {settings.SolutionPath}");
            }
                
            var ext = Path.GetExtension(settings.SolutionPath).ToLowerInvariant();
            if (ext != ".sln" && ext != ".slnx")
            {
                return ($"Unsupported solution file extension: {ext}. Only .sln and .slnx are supported.");
            }
        }
        else
        {
            if (!File.Exists(settings.CsprojPath))
                return ($"Project file not found: {settings.CsprojPath}");
        }
        return null;
    }

    /// <summary>
    /// Displays the reference trees for each root project using <see cref="ProjectManipulator.PrintReferenceTree"/>.
    /// </summary>
    /// <param name="displayGraph">The dependency graph to display.</param>
    /// <param name="allProjects">All project paths.</param>
    private static void DisplayReferenceTrees(Dictionary<string, List<string>> displayGraph, HashSet<string> allProjects)
    {
        var displayReferencedProjects = new HashSet<string>();
        foreach (var r in displayGraph.Values.SelectMany(refs => refs).Where(r => r.EndsWith(CsProj)))
        {
            displayReferencedProjects.Add(r);
        }
        var displayRootProjects = allProjects.Except(displayReferencedProjects).ToList();
        foreach (var proj in displayRootProjects)
        {
            var tree = new Tree($"[bold]{Path.GetFileName(proj)}[/]");
            ProjectManipulator.PrintReferenceTree(displayGraph, proj, tree, []);
            AnsiConsole.Write(tree);
        }
    }

    /// <summary>
    /// Outputs the dependency graph as a Markdown file with Mermaid syntax using <see cref="GraphMarkdownUtil.GenerateMermaidMarkdown"/>.
    /// </summary>
    /// <param name="settings">The command settings. See <see cref="Settings"/>.</param>
    /// <param name="displayGraph">The dependency graph to display.</param>
    /// <param name="allProjects">All project paths.</param>
    /// <param name="rootPath">The root solution or project path.</param>
    private static void OutputDependencyGraphMarkdown(
        Settings settings,
        Dictionary<string, List<string>> displayGraph,
        HashSet<string> allProjects,
        string rootPath)
    {
        var solutionDir = Path.GetDirectoryName(rootPath);
        var outputPath = Path.IsPathRooted(settings.GraphMdPath)
            ? settings.GraphMdPath
            : Path.Combine(solutionDir ?? string.Empty, settings.GraphMdPath);
        var outputFileName = Path.GetFileName(outputPath);
        var mdReferencedProjects = new HashSet<string>();
        
        foreach (var r in displayGraph.Values.SelectMany(refs => refs).Where(r => r.EndsWith(CsProj)))
        {
            mdReferencedProjects.Add(r);
        }
        
        List<string> mdRootProjects;
        string mdTitle;
        bool hasCsproj = !string.IsNullOrWhiteSpace(settings.CsprojPath);
        if (hasCsproj)
        {
            mdRootProjects = [settings.CsprojPath];
            mdTitle = Path.GetFileNameWithoutExtension(settings.CsprojPath);
        }
        else
        {
            mdRootProjects = allProjects.Except(mdReferencedProjects).ToList();
            mdTitle = mdRootProjects.Count > 0 ? Path.GetFileNameWithoutExtension(mdRootProjects[0]) : "dependencies";
        }
        
        var mermaidMd = GraphMarkdownUtil.GenerateMermaidMarkdown(displayGraph, mdTitle, outputFileName, mdRootProjects);
        File.WriteAllText(outputPath, mermaidMd);
        AnsiConsole.MarkupLine($"[green]Dependency graph written to:[/] {outputPath}");
    }
}
