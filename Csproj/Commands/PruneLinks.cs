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

    public override int Execute(CommandContext context, Settings settings)
    {
        // Validate that exactly one of --solution or --csproj is provided
        bool hasSolution = !string.IsNullOrWhiteSpace(settings.SolutionPath);
        bool hasCsproj = !string.IsNullOrWhiteSpace(settings.CsprojPath);
        if (hasSolution == hasCsproj)
        {
            AnsiConsole.MarkupLine("[red]You must specify either --solution or --csproj, but not both.[/]");
            return -1;
        }

        List<string> projects;
        string rootPath;
        if (hasSolution)
        {
            if (!File.Exists(settings.SolutionPath))
            {
                AnsiConsole.MarkupLine($"[red]Solution file not found: {settings.SolutionPath}[/]");
                return -1;
            }
            var ext = Path.GetExtension(settings.SolutionPath).ToLowerInvariant();
            if (ext != ".sln" && ext != ".slnx")
            {
                AnsiConsole.MarkupLine($"[red]Unsupported solution file extension: {ext}. Only .sln and .slnx are supported.[/]");
                return -1;
            }
            projects = SolutionFileParser.GetAllProjectPaths(settings.SolutionPath).ToList();
            rootPath = settings.SolutionPath;
        }
        else
        {
            if (!File.Exists(settings.CsprojPath))
            {
                AnsiConsole.MarkupLine($"[red]Project file not found: {settings.CsprojPath}[/]");
                return -1;
            }
            // Recursively collect all referenced projects
            projects = ProjectManipulator.CollectAllReferencedProjects(settings.CsprojPath);
            rootPath = settings.CsprojPath;
        }

        var originalGraph = ProjectManipulator.BuildDependencyGraph(projects);
        var allProjects = projects.ToHashSet();
        var changes = ProjectManipulator.PruneRedundantLinks(originalGraph, settings.DryRun, settings.Backup);
        var displayGraph = settings.DryRun ? originalGraph : ProjectManipulator.BuildDependencyGraph(projects);
        if (!settings.DryRun)
        {
            displayGraph = ProjectManipulator.BuildDependencyGraph(projects);
        }

        if (settings.Verbose)
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

        // Determine output file path for --graph-md
        if (!string.IsNullOrWhiteSpace(settings.GraphMdPath))
        {
            var solutionDir = Path.GetDirectoryName(rootPath);
            var outputPath = Path.IsPathRooted(settings.GraphMdPath)
                ? settings.GraphMdPath
                : Path.Combine(solutionDir ?? string.Empty, settings.GraphMdPath);
            var outputFileName = Path.GetFileName(outputPath);
            // Use displayGraph for Markdown
            var mdReferencedProjects = new HashSet<string>();
            foreach (var r in displayGraph.Values.SelectMany(refs => refs).Where(r => r.EndsWith(CsProj)))
            {
                mdReferencedProjects.Add(r);
            }
            List<string> mdRootProjects;
            string mdTitle;
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

        if (changes.Count == 0)
            AnsiConsole.MarkupLine("[green]No redundant links found.[/]");
        else
        {
            foreach (var change in changes)
                AnsiConsole.MarkupLine($"[yellow]{change}[/]");
        }

        return 0;
    }
}
