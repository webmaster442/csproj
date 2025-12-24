using System.ComponentModel;

using CsProj.Core;
using CsProj.Domain;

using Spectre.Console;
using Spectre.Console.Cli;

namespace CsProj.Commands.Modify;

internal sealed class PruneLinksCommand : BaseModifyProjectsCommand<PruneLinksCommand.Settings>
{
    public class Settings : BaseModifySettings
    {
        [Description("Dryrun mode. Only show what would be changed.")]
        [CommandOption("-D|--dryrun")]
        public bool DryRun { get; set; }

        [Description("Show the reference tree for each project.")]
        [CommandOption("-v|--verbose")]
        public bool Verbose { get; set; }

        [Description("Output the dependency graph as a Markdown file with Mermaid syntax.")]
        [CommandOption("--graph-md")]
        public string GraphMdPath { get; set; } = string.Empty;
    }

    private const string CsProj = ".csproj";

    public PruneLinksCommand(ILogger logger,
                             IAnsiConsole console,
                             TimeProvider timeProvider)
        : base(logger, console, timeProvider)
    {
    }


    protected override bool TryModifyProjects(IEnumerable<CsharpProject> sdkprojects, Settings settings)
    {
        var originalGraph = BuildDependencyGraph(sdkprojects);
        var changes = PruneRedundantLinks(originalGraph, sdkprojects, settings.DryRun, settings.CreateBackup);
        var displayGraph = settings.DryRun ? originalGraph : BuildDependencyGraph(sdkprojects);
        var allProjects = sdkprojects.Select(x => x.AbsolutePath).ToHashSet();

        if (settings.Verbose)
            DisplayReferenceTrees(displayGraph, allProjects);

        string rootPath;

        if (File.Exists(settings.Path))
            rootPath = Path.GetDirectoryName(settings.Path)!;
        else
            rootPath = settings.Path;

        if (!string.IsNullOrWhiteSpace(settings.GraphMdPath))
            OutputDependencyGraphMarkdown(settings, displayGraph, allProjects, rootPath);


        return sdkprojects.Any(x => x.WasModified);
    }

    private static Dictionary<string, List<string>> BuildDependencyGraph(IEnumerable<CsharpProject> projects)
    {
        var graph = new Dictionary<string, List<string>>();
        foreach (var project in projects)
        {
            var refs = project.GetRawProjectAndPackageReferences().ToList();
            graph[project.AbsolutePath] = refs;
        }
        return graph;
    }

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
            PrintReferenceTree(displayGraph, proj, tree, []);
            AnsiConsole.Write(tree);
        }
    }

    private static void OutputDependencyGraphMarkdown(Settings settings,
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

        List<string> mdRootProjects = allProjects.Except(mdReferencedProjects).ToList();
        string mdTitle = mdRootProjects.Count > 0 ? Path.GetFileNameWithoutExtension(mdRootProjects[0]) : "dependencies";

        var mermaidMd = Markdown.GenerateMermaidMarkdown(displayGraph, mdTitle, outputFileName, mdRootProjects);
        File.WriteAllText(outputPath, mermaidMd);
        AnsiConsole.MarkupLine($"[green]Dependency graph written to:[/] {outputPath}");
    }

    public static List<string> PruneRedundantLinks(Dictionary<string, List<string>> graph,
                                                   IEnumerable<CsharpProject> projects,
                                                   bool dryRun,
                                                   bool backup)
    {
        static bool IsReachable(Dictionary<string, List<string>> graph,
                                string from,
                                string target,
                                HashSet<string> visited,
                                bool skipDirect)
        {
            foreach (var next in graph[from])
            {
                if (next == target && !skipDirect) return true;
                if (next == target && skipDirect) continue;
                if (next.EndsWith(".csproj") && visited.Add(next))
                {
                    if (IsReachable(graph, next, target, visited, false)) return true;
                }
            }
            return false;
        }

        static bool IsNuGetReachable(Dictionary<string, List<string>> graph,
                                     string from,
                                     string nuget,
                                     HashSet<string> visited)
        {
            if (!graph.TryGetValue(from, out List<string>? refs) || !visited.Add(from))
            {
                return false;
            }

            return refs.Any(r => r == nuget)
                   || refs
                       .Where(r => r.EndsWith(".csproj"))
                       .Any(refProj => IsNuGetReachable(graph, refProj, nuget, visited));
        }

        CsharpProject GetProject(string projectPath)
            => projects.First(p => p.AbsolutePath == projectPath);

        var changes = new List<string>();
        foreach (var proj in graph.Keys)
        {
            var directRefs = graph[proj].Where(r => r.EndsWith(".csproj")).ToList();
            var directNuGets = graph[proj].Where(r => !r.EndsWith(".csproj")).ToList();

            // Remove redundant project references
            foreach (var refProj in directRefs
                         .Where(refProj => IsReachable(graph, proj, refProj, [proj], skipDirect: true)))
            {
                changes.Add($"Redundant project reference: {proj} -> {refProj}");
                if (!dryRun)
                {
                    GetProject(proj).RemoveProjectReference(refProj);
                }
            }

            // Remove redundant NuGet references
            foreach (var nuget in directNuGets
                         .Where(nuget => directRefs.Any(refProj => IsNuGetReachable(graph, refProj, nuget, [proj]))))
            {
                changes.Add($"Redundant NuGet reference: {proj} -> {nuget}");
                if (!dryRun)
                {
                    GetProject(proj).RemovePackageReference(nuget);
                }
            }
        }
        return changes;
    }

    public static void PrintReferenceTree(Dictionary<string, List<string>> graph,
                                          string proj,
                                          IHasTreeNodes parent,
                                          HashSet<string> visited)
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

}
