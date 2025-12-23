using System.Xml.Linq;

namespace CsProj.Core;

internal static class SolutionFileParser
{
    private static IEnumerable<string> GetProjectsFromSlnx(TextReader textReader, string projectExtension, string solutionFolder)
    {
        XDocument xml = XDocument.Parse($"<Solution>{textReader.ReadToEnd()}");
        var projectElements = xml.Root?.Elements().Where(element => element.Name == "Project") ?? Enumerable.Empty<XElement>();
        foreach (var projectElement in projectElements)
        {
            string? path = projectElement.Attribute("Path")?.Value;
            if (path != null && Path.GetExtension(path).Equals(projectExtension, StringComparison.OrdinalIgnoreCase))
            {
                yield return Path.GetFullPath(Path.Combine(solutionFolder, path));
            }
        }
    }

    private static IEnumerable<string> GetProjectsFromSln(TextReader solutionContents, string projectExtension, string solutionFolder)
    {
        string? line = null;
        while ((line = solutionContents.ReadLine()) != null)
        {
            if (line.StartsWith("Project("))
            {
                string[] parts = line.Split(',');
                string fileName = parts[1][2..^1];
                if (Path.GetExtension(fileName).Equals(projectExtension, StringComparison.OrdinalIgnoreCase))
                {
                    yield return Path.GetFullPath(Path.Combine(solutionFolder, fileName));
                }
            }
        }
    }

    internal static IEnumerable<string> GetProjects(TextReader solutionContents, string projectExtension, string solutionFolder)
    {
        string? firstLine = solutionContents.ReadLine();

        return firstLine == null
            ? Enumerable.Empty<string>()
            : firstLine.StartsWith("<Solution>")
                ? GetProjectsFromSlnx(solutionContents, projectExtension, solutionFolder)
                : GetProjectsFromSln(solutionContents, projectExtension, solutionFolder);
    }

    public static IReadOnlyList<string> GetProjectAbsolutePaths(string solutionPath)
    {
        using var reader = new StreamReader(solutionPath);
        var folder = Path.GetDirectoryName(solutionPath) ?? "";
        return GetProjects(reader, ".csproj", folder).ToList();
    }
}
