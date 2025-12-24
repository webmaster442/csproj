using System.Text;

namespace CsProj.Core;

internal static class Markdown
{
    public static string GenerateMermaidMarkdown(Dictionary<string, List<string>> graph,
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

    private static void WriteMermaidEdges(Dictionary<string, List<string>> graph,
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
