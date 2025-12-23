using System.Collections;
using System.Text;

namespace CsProj.Core;

internal sealed class DependencyTree : IEnumerable<KeyValuePair<string, HashSet<string>>>
{
    private readonly Dictionary<string, HashSet<string>> _dependencies;

    private DependencyTree()
    {
        _dependencies = new Dictionary<string, HashSet<string>>();
    }

    public bool ContainsKey(string key)
        => _dependencies.ContainsKey(key);

    public HashSet<string> this[string key]
        => _dependencies[key];

    public static DependencyTree CreateProjectTree(IEnumerable<IReadonlyCsharpProject> projects)
    {
        DependencyTree result = new();
        Dictionary<string, IReadonlyCsharpProject> projectLookup = projects.ToDictionary(p => p.AbsolutePath, p => p);
        foreach (var project in projects)
        {
            var projectRefs = project.GetProjectReferencesAbsolutePath();
            foreach (var projRef in projectRefs)
            {
                if (projectLookup.ContainsKey(projRef))
                {
                    if (!result._dependencies.TryGetValue(project.AbsolutePath, out HashSet<string>? value))
                    {
                        value = new HashSet<string>();
                        result._dependencies[project.AbsolutePath] = value;
                    }

                    value.Add(projRef);
                }
            }
        }
        return result;
    }

    public static DependencyTree CreatePackageReferenceTree(IEnumerable<IReadonlyCsharpProject> projects)
    {
        DependencyTree result = new();
        Dictionary<string, IReadonlyCsharpProject> projectLookup = projects.ToDictionary(p => p.AbsolutePath, p => p);
        foreach (var project in projects)
        {
            var packageRefs = project.GetPackageReferences();
            foreach (var packageRef in packageRefs)
            {
                string packageNode = $"{packageRef.PackageName} - {packageRef.Version}";
                if (!result._dependencies.TryGetValue(project.AbsolutePath, out HashSet<string>? value))
                {
                    value = new HashSet<string>();
                    result._dependencies[project.AbsolutePath] = value;
                }

                value.Add(packageNode);
            }
        }
        return result;
    }

    public string ToMermaid()
    {
        StringBuilder sb = new();
        sb.AppendLine("graph TD");
        foreach (var keyValuePair in _dependencies)
        {
            var from = Path.GetFileNameWithoutExtension(keyValuePair.Key);
            foreach (var toProj in keyValuePair.Value)
            {
                var to = Path.GetFileNameWithoutExtension(toProj);
                sb.AppendLine($"    {from} --> {to}");
            }
        }
        return sb.ToString();
    }

    public string ToNomnoml()
    {
        StringBuilder sb = new();
        sb.AppendLine("#direction: down");
        foreach (var keyValuePair in _dependencies)
        {
            var from = Path.GetFileNameWithoutExtension(keyValuePair.Key);
            foreach (var toProj in keyValuePair.Value)
            {
                var to = Path.GetFileNameWithoutExtension(toProj);
                sb.AppendLine($"[{from}] -> [{to}]");
            }
        }
        return sb.ToString();
    }

    public IEnumerator<KeyValuePair<string, HashSet<string>>> GetEnumerator()
        => _dependencies.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}
