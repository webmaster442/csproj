using System.Xml.Linq;

// ReSharper disable InvertIf

namespace Csproj.DomainServices;

internal static class SolutionFileParser
{
    /// <summary>
    /// Parses a solution file and retrieves the paths of all projects with the specified extension.
    /// </summary>
    /// <param name="solutionContents">The contents of the solution file as a <see cref="TextReader"/>.</param>
    /// <param name="projectExtension">The file extension of the projects to retrieve (e.g., ".csproj").</param>
    /// <param name="solutionFolder">The folder containing the solution file.</param>
    /// <returns>An enumerable of full paths to the projects with the specified extension.</returns>
    public static IEnumerable<string> GetProjects(
        TextReader solutionContents,
        string projectExtension,
        string solutionFolder)
    {
        string? firstLine = solutionContents.ReadLine();

        if (firstLine is null)
        {
            return [];
        }
        
        // Determines the solution type and calls the appropriate parser.
        return firstLine.StartsWith("<Solution>")
            ? GetProjectsFromSlnx(solutionContents, projectExtension, solutionFolder)
            : GetProjectsFromSln(solutionContents, projectExtension, solutionFolder);
    }

    /// <summary>
    /// Parses a .slnx solution file and retrieves the paths of all projects with the specified extension.
    /// </summary>
    /// <param name="textReader">The contents of the .slnx file as a <see cref="TextReader"/>.</param>
    /// <param name="projectExtension">The file extension of the projects to retrieve (e.g., ".csproj").</param>
    /// <param name="solutionFolder">The folder containing the solution file.</param>
    /// <returns>An enumerable of full paths to the projects with the specified extension.</returns>
    private static IEnumerable<string> GetProjectsFromSlnx(
        TextReader textReader,
        string projectExtension,
        string solutionFolder)
    {
        XDocument xml = XDocument.Parse($"<Solution>{textReader.ReadToEnd()}");
        // Recursively find all <Project> elements
        var projectElements = xml.Descendants("Project");
        foreach (var projectElement in projectElements)
        {
            string? path = projectElement.Attribute("Path")?.Value;
            if (path != null && Path.GetExtension(path).Equals(projectExtension, StringComparison.OrdinalIgnoreCase))
            {
                yield return Path.GetFullPath(Path.Combine(solutionFolder, path));
            }
        }
    }

    /// <summary>
    /// Parses a .sln solution file and retrieves the paths of all projects with the specified extension.
    /// </summary>
    /// <param name="solutionContents">The contents of the .sln file as a <see cref="TextReader"/>.</param>
    /// <param name="projectExtension">The file extension of the projects to retrieve (e.g., ".csproj").</param>
    /// <param name="solutionFolder">The folder containing the solution file.</param>
    /// <returns>An enumerable of full paths to the projects with the specified extension.</returns>
    private static IEnumerable<string> GetProjectsFromSln(
        TextReader solutionContents,
        string projectExtension,
        string solutionFolder)
    {
        while (solutionContents.ReadLine() is { } line)
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

    /// <summary>
    /// Retrieves the paths of all projects in a solution file.
    /// </summary>
    /// <param name="solutionPath">The full path to the solution file.</param>
    /// <returns>An enumerable of full paths to the projects in the solution file.</returns>
    public static IEnumerable<string> GetAllProjectPaths(string solutionPath)
    {
        using var reader = new StreamReader(solutionPath);
        var folder = Path.GetDirectoryName(solutionPath) ?? "";
        return GetProjects(reader, ".csproj", folder).ToList();
    }
}
