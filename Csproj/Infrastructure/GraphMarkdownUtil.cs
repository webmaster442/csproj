using System.Text;

namespace Csproj.Infrastructure;

public static class GraphMarkdownUtil
{
    /// <summary>
/// Generates a Mermaid Markdown representation of a project dependency graph.
/// </summary>
/// <param name="graph">A <see cref="Dictionary{TKey,TValue}"/> representing the project dependency graph, where keys are project paths and values are lists of referenced project paths.</param>
/// <param name="title">The title of the Mermaid graph.</param>
/// <param name="fileName">The name of the file to be displayed as the Markdown header.</param>
/// <param name="rootProjects">A <see cref="List{T}"/> of root projects to start the graph traversal from.</param>
/// <returns>A <see cref="string"/> containing the Mermaid Markdown representation of the graph.</returns>
/// <seealso cref="WriteMermaidEdges(Dictionary{string, List{string}}, string, StringBuilder, HashSet{string})"/>
    public static string GenerateMermaidMarkdown(
        Dictionary<string, List<string>> graph,
        string title,
        string fileName,
        List<string> rootProjects)
    {
        var sb = new StringBuilder();
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

    /// <summary>
    /// Recursively writes edges of the Mermaid graph for a given project and its dependencies.
    /// </summary>
    /// <param name="graph">A <see cref="Dictionary{TKey,TValue}"/> representing the project dependency graph.</param>
    /// <param name="proj">The current project being processed.</param>
    /// <param name="sb">The <see cref="StringBuilder"/> used to construct the Mermaid graph.</param>
    /// <param name="visited">A <see cref="HashSet{T}"/> of already visited projects to avoid infinite recursion.</param>
    /// <seealso cref="GenerateMermaidMarkdown(Dictionary{string, List{string}}, string, string, List{string})"/>
    private static void WriteMermaidEdges(
        Dictionary<string, List<string>> graph,
        string proj,
        StringBuilder sb,
        HashSet<string> visited)
    {
        if (!visited.Add(proj)) return;
        var from = Path.GetFileNameWithoutExtension(proj);
        if (!graph.TryGetValue(proj, out var refs)) return;
        foreach (var toProj in refs.Where(x => x.EndsWith(".csproj")))
        {
            var to = Path.GetFileNameWithoutExtension(toProj);
            sb.AppendLine($"    {from} --> {to}");
            WriteMermaidEdges(graph, toProj, sb, visited);
        }
    }
}

