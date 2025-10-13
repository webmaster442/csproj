using System.ComponentModel;
using System.Xml;

using Spectre.Console.Cli;
using Spectre.Console;
using Csproj.DomainServices;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace Csproj.Commands;

internal sealed class PruneLinks : Command<PruneLinks.Settings>
{
    private const string CsProj = ".csproj";
    
    public class Settings : CommandSettings
    {
        [Description("Solution file path (.sln)")]
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
            projects = CollectAllReferencedProjects(settings.CsprojPath);
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
                PrintReferenceTree(displayGraph, proj, tree, []);
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
            var mermaidMd = GenerateMermaidMarkdown(displayGraph, mdTitle, outputFileName, mdRootProjects);
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

    private static string GenerateMermaidMarkdown(Dictionary<string, List<string>> graph, string title, string fileName, List<string> rootProjects)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"# {fileName}\n");
        sb.AppendLine("````mermaid");
        sb.AppendLine("---");
        sb.AppendLine($"title: {title}");
        sb.AppendLine("---");
        sb.AppendLine("graph TD");
        var visited = new HashSet<string>();
        foreach (var root in rootProjects)
        {
            WriteMermaidEdges(graph, root, sb, visited);
        }
        sb.AppendLine("````");
        return sb.ToString();
    }

    private static void WriteMermaidEdges(Dictionary<string, List<string>> graph, string proj, System.Text.StringBuilder sb, HashSet<string> visited)
    {
        if (!visited.Add(proj)) return;
        var from = Path.GetFileNameWithoutExtension(proj);
        if (!graph.TryGetValue(proj, out var refs)) return;
        foreach (var toProj in refs.Where(x => x.EndsWith(CsProj)))
        {
            var to = Path.GetFileNameWithoutExtension(toProj);
            sb.AppendLine($"    {from} --> {to}");
            WriteMermaidEdges(graph, toProj, sb, visited);
        }
    }

    private static List<string> CollectAllReferencedProjects(string rootCsproj)
    {
        var found = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var stack = new Stack<string>();
        stack.Push(rootCsproj);
        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (!found.Add(current)) continue;
            foreach (var reference in GetProjectReferencesFromFile(current)
                         .Where(reference => reference.EndsWith(CsProj, StringComparison.OrdinalIgnoreCase)
                                             && !found.Contains(reference)
                                             && File.Exists(reference)))
            {
                stack.Push(reference);
            }
        }
        return found.ToList();
    }

    // Helper to parse .csproj and get all referenced .csproj files (absolute paths)
    private static List<string> GetProjectReferencesFromFile(string csprojPath)
    {
        var references = new List<string>();
        try
        {
            var doc = new XmlDocument();
            doc.Load(csprojPath);
            var nodes = doc.SelectNodes("//ProjectReference[@Include]");
            if (nodes != null)
            {
                var baseDir = Path.GetDirectoryName(csprojPath) ?? string.Empty;
                references.AddRange(
                    nodes.OfType<XmlNode>()
                        .Select(node => node.Attributes?["Include"]?.Value)
                        .Where(include => !string.IsNullOrWhiteSpace(include))
                        .Select(include => Path.GetFullPath(Path.Combine(baseDir, include!)))
                );
            }
        }
        catch
        {
            // Ignore parse errors, treat as no references
        }
        return references;
    }
}
