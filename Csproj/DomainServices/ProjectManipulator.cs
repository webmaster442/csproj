using System.Xml;
using System.Xml.Linq;

using Spectre.Console;

// ReSharper disable once InvertIf

namespace Csproj.DomainServices;

internal class ProjectManipulator
{
    private readonly XDocument _project;
    private readonly string _projectName;

    public bool WasModified { get; private set; }

    public ProjectManipulator(XDocument project, string projectName)
    {
        _project = project;
        _projectName = projectName;
        WasModified = false;
    }

    public bool IsSdkStyleProject()
    {
        var sdkAttribute = _project?.Element("Project")?.Attribute("Sdk");
        return sdkAttribute != null && !string.IsNullOrWhiteSpace(sdkAttribute.Value);
    }

    public ProjectManipulator SetNullable(bool enabled)
    {
        var value = enabled ? "enable" : "disable";
        var nullableElement = _project.Element("Project")?.Element("PropertyGroup")?.Element("Nullable");
        if (nullableElement == null)
        {
            _project.Element("Project")?.Element("PropertyGroup")?.Add(new XElement("Nullable", value));
        }
        else
        {
            nullableElement.Value = value;
        }

        WasModified = true;
        return this;
    }

    public ProjectManipulator SetTargetFramework(string targetFramework, string? oldFramework = null)
    {
        var targetFrameworkElement = _project.Element("Project")?.Element("PropertyGroup")?.Element("TargetFramework");
        if (targetFrameworkElement == null)
        {
            throw new InvalidOperationException("TargetFramework element not found in the project file.");
        }

        if (!string.IsNullOrEmpty(oldFramework))
        {
            if (targetFrameworkElement.Value == oldFramework)
            {
                targetFrameworkElement.Value = targetFramework;
                WasModified = true;
            }
        }
        else
        {
            targetFrameworkElement.Value = targetFramework;
            WasModified = true;
        }
        return this;
    }

    public ProjectManipulator SetVersion(string versionString)
    {
        var versionElement = _project.Element("Project")?.Element("PropertyGroup")?.Element("Version");
        if (versionElement == null)
        {
            _project.Element("Project")?.Element("PropertyGroup")?.Add(new XElement("Version", versionString));
        }
        else
        {
            versionElement.Value = versionString;
        }

        WasModified = true;
        return this;
    }

    public ProjectManipulator SetAssemblyVersion(string versionString)
    {
        var assemblyVersionElement = _project.Element("Project")?.Element("PropertyGroup")?.Element("AssemblyVersion");
        if (assemblyVersionElement == null)
        {
            _project.Element("Project")?.Element("PropertyGroup")?.Add(new XElement("AssemblyVersion", versionString));
        }
        else
        {
            assemblyVersionElement.Value = versionString;
        }

        WasModified = true;
        return this;
    }

    public ProjectManipulator SetFileVersion(string versionString)
    {
        var fileVersionElement = _project.Element("Project")?.Element("PropertyGroup")?.Element("FileVersion");
        if (fileVersionElement == null)
        {
            _project.Element("Project")?.Element("PropertyGroup")?.Add(new XElement("FileVersion", versionString));
        }
        else
        {
            fileVersionElement.Value = versionString;
        }

        WasModified = true;
        return this;
    }

    public string GetXml()
        => _project.ToString();

    public static Dictionary<string, List<string>> BuildDependencyGraph(IEnumerable<string> projectPaths)
    {
        var graph = new Dictionary<string, List<string>>();
        foreach (var path in projectPaths)
        {
            var doc = XDocument.Load(path);
            var refs = doc.Descendants("ProjectReference")
                .Select(pr => pr.Attribute("Include")?.Value)
                .Where(include => !string.IsNullOrEmpty(include))
                .Select(include => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path)!, include!)))
                .ToList();
            
            refs.AddRange(
                doc
                    .Descendants("PackageReference")
                    .Select(nr => nr.Attribute("Include")?.Value)
                    .Where(pkg => !string.IsNullOrEmpty(pkg))!);
            
            graph[path] = refs;
        }
        return graph;
    }

    public static List<string> PruneRedundantLinks(Dictionary<string, List<string>> graph, bool dryRun, bool backup)
    {
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
                    RemoveProjectReference(proj, refProj, backup);
                }
            }

            // Remove redundant NuGet references
            foreach (var nuget in directNuGets
                         .Where(nuget => directRefs.Any(refProj => IsNuGetReachable(graph, refProj, nuget, [proj]))))
            {
                changes.Add($"Redundant NuGet reference: {proj} -> {nuget}");
                if (!dryRun)
                {
                    RemoveNuGetReference(proj, nuget, backup);
                }
            }
        }
        return changes;
    }

    private static bool IsReachable(Dictionary<string, List<string>> graph, string from, string target, HashSet<string> visited, bool skipDirect)
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

    private static void RemoveProjectReference(string projPath, string refProjPath, bool backup)
    {
        var doc = XDocument.Load(projPath);
        var refs = doc
            .Descendants("ProjectReference")
            .Where(pr => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(projPath)!, pr.Attribute("Include")?.Value ?? "")) == refProjPath)
            .ToList();
        
        foreach (var pr in refs)
        {
            pr.Remove();
        }
        
        if (backup)
        {
            File.Copy(projPath, projPath + ".bak", overwrite:true);
        }
        
        doc.Save(projPath);
    }
    
    private static bool IsNuGetReachable(Dictionary<string, List<string>> graph, string from, string nuget, HashSet<string> visited)
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
    
    private static void RemoveNuGetReference(string projPath, string nuget, bool backup)
    {
        var doc = XDocument.Load(projPath);
        var toRemove = doc.Descendants("PackageReference")
            .Where(pr => pr.Attribute("Include")?.Value == nuget)
            .ToList();

        if (toRemove.Count == 0)
        {
            return;
        }
        
        foreach (var node in toRemove)
        {
            node.Remove();
        }
        
        if (backup)
        {
            File.Copy(projPath, projPath + ".bak", true);
        }
        
        doc.Save(projPath);
    }

    /// <summary>
    /// Collects all referenced projects starting from the specified root .csproj file.
    /// </summary>
    /// <param name="rootCsproj">The path to the root .csproj file.</param>
    /// <returns>A <see cref="List{string}"/> containing all referenced project file paths.</returns>
    /// <seealso cref="GetProjectReferencesFromFile(string)"/>
    public static List<string> CollectAllReferencedProjects(string rootCsproj)
    {
        var found = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var stack = new Stack<string>();
        stack.Push(rootCsproj);
        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (!found.Add(current)) continue;
            foreach (var reference in GetProjectReferencesFromFile(current)
                         .Where(reference => reference.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase)
                                             && !found.Contains(reference)
                                             && File.Exists(reference)))
            {
                stack.Push(reference);
            }
        }
        return found.ToList();
    }

    /// <summary>
    /// Retrieves the project references from a given .csproj file.
    /// </summary>
    /// <param name="csprojPath">The path to the .csproj file.</param>
    /// <returns>A <see cref="List{string}"/> of project reference file paths.</returns>
    /// <seealso cref="CollectAllReferencedProjects(string)"/>
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

    /// <summary>
    /// Prints the project reference tree starting from the specified project.
    /// </summary>
    /// <param name="graph">A <see cref="Dictionary{string, List{string}}"/> representing the project dependency graph.</param>
    /// <param name="proj">The current project being processed.</param>
    /// <param name="parent">The parent node in the tree structure.</param>
    /// <param name="visited">A <see cref="HashSet{string}"/> of already visited projects to avoid infinite recursion.</param>
    /// <seealso cref="CollectAllReferencedProjects(string)"/>
    public static void PrintReferenceTree(
        Dictionary<string, List<string>> graph,
        string proj,
        object parent,
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
