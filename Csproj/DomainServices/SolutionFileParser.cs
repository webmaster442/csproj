using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Csproj.DomainServices;
internal static class SolutionFileParser
{
    public static IEnumerable<string> GetProjects(TextReader solutionContents, string projectExtension, string solutionFolder)
    {
        string? firstLine = solutionContents.ReadLine();

        return firstLine == null
            ? Enumerable.Empty<string>()
            : firstLine.StartsWith("<Solution>")
                ? GetProjectsFromSlnx(solutionContents, projectExtension, solutionFolder)
                : GetProjectsFromSln(solutionContents, projectExtension, solutionFolder);
    }

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
}
