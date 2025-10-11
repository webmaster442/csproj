using System.ComponentModel;
using Spectre.Console.Cli;
using Spectre.Console;
using Csproj.DomainServices;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace Csproj.Commands;

internal sealed class PruneLinks : Command<PruneLinks.Settings>
{
    public class Settings : CommandSettings
    {
        [Description("Solution file path (.sln)")]
        [CommandOption("-s|--solution")]
        public string SolutionPath { get; set; } = string.Empty;

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
        if (string.IsNullOrWhiteSpace(settings.SolutionPath) || !File.Exists(settings.SolutionPath))
        {
            AnsiConsole.MarkupLine($"[red]Solution file not found: {settings.SolutionPath}[/]");
            return -1;
        }

        var projects = SolutionFileParser.GetAllProjectPaths(settings.SolutionPath).ToList();
        var originalGraph = ProjectManipulator.BuildDependencyGraph(projects);

        // Find root project(s) once
        var allProjects = projects.ToHashSet();

        // Prune redundant links and get updated graph
        var changes = ProjectManipulator.PruneRedundantLinks(originalGraph, settings.DryRun, settings.Backup);
        var displayGraph = settings.DryRun ? originalGraph : ProjectManipulator.BuildDependencyGraph(projects); // Rebuild after prune

        if (!settings.DryRun)
        {
            // Rebuild graph after pruning
            displayGraph = ProjectManipulator.BuildDependencyGraph(projects);
        }

        if (settings.Verbose)
        {
            var displayReferencedProjects = new HashSet<string>();
            foreach (var r in displayGraph.Values.SelectMany(refs => refs).Where(r => r.EndsWith(".csproj")))
            {
                displayReferencedProjects.Add(r);
            }
            var displayRootProjects = allProjects.Except(displayReferencedProjects).ToList();
            foreach (var proj in displayRootProjects)
            {
                var tree = new Tree($"[bold]{Path.GetFileName(proj)}[/]");
                PrintReferenceTree(displayGraph, proj, tree, []);
                AnsiConsole.Write(tree);
            }
        }

        // Determine output file path for --graph-md
        string? outputPath;
        var solutionDir = Path.GetDirectoryName(settings.SolutionPath);
        if (!string.IsNullOrWhiteSpace(settings.GraphMdPath))
        {
            outputPath = Path.IsPathRooted(settings.GraphMdPath)
                ? settings.GraphMdPath
                : Path.Combine(solutionDir ?? string.Empty, settings.GraphMdPath);
        }
        else
        {
            var solutionName = Path.GetFileNameWithoutExtension(settings.SolutionPath);
            outputPath = Path.Combine(solutionDir ?? string.Empty, solutionName + ".md");
        }

        if (!string.IsNullOrWhiteSpace(outputPath))
        {
            var outputFileName = Path.GetFileName(outputPath);
            // Use displayGraph for Markdown
            var mdReferencedProjects = new HashSet<string>();
            foreach (var r in displayGraph.Values.SelectMany(refs => refs).Where(r => r.EndsWith(".csproj")))
            {
                mdReferencedProjects.Add(r);
            }
            var mdRootProjects = allProjects.Except(mdReferencedProjects).ToList();
            var mdTitle = mdRootProjects.Count > 0 ? Path.GetFileNameWithoutExtension(mdRootProjects[0]) : "dependencies";
            var mermaidMd = GenerateMermaidMarkdown(displayGraph, mdTitle, outputFileName);
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

    private static void PrintReferenceTree(Dictionary<string, List<string>> graph, string proj, object parent, HashSet<string> visited)
    {
        visited.Add(proj);
        foreach (var reference in graph[proj])
        {
            var nodeLabel = reference.EndsWith(".csproj") ? Path.GetFileName(reference) : reference;
            TreeNode child;
            switch (parent)
            {
                case Tree tree:
                    child = tree.AddNode(nodeLabel);
                    break;
                case TreeNode node:
                    child = node.AddNode(nodeLabel);
                    break;
                default:
                    continue;
            }
            if (reference.EndsWith(".csproj") && !visited.Contains(reference) && graph.ContainsKey(reference))
            {
                PrintReferenceTree(graph, reference, child, visited);
            }
        }
    }

    private static string GenerateMermaidMarkdown(Dictionary<string, List<string>> graph, string title, string fileName)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"# {fileName}\n");
        sb.AppendLine("````mermaid");
        sb.AppendLine("---");
        sb.AppendLine($"title: {title}");
        sb.AppendLine("---");
        sb.AppendLine("graph TD");
        foreach (var kvp in graph)
        {
            var from = Path.GetFileNameWithoutExtension(kvp.Key);
            foreach (var toProj in kvp.Value.Where(x => x.EndsWith(".csproj")))
            {
                var to = Path.GetFileNameWithoutExtension(toProj);
                sb.AppendLine($"    {from} --> {to}");
            }
        }
        sb.AppendLine("````");
        return sb.ToString();
    }
}
