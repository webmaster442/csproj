using Csproj.Domain;
using Csproj.Domain.Interfaces;
using Csproj.DomainServices;

namespace Csproj.Infrastructure;

internal static class ProjectProvider
{
    private static IReadOnlyList<string> GetSolutions(string path)
    {
        List<string> solutions = new List<string>();
        solutions.AddRange(Directory.GetFiles(path, "*.sln", SearchOption.TopDirectoryOnly));
        solutions.AddRange(Directory.GetFiles(path, "*.slnx", SearchOption.TopDirectoryOnly));
        return solutions;
    }

    public static ProjectsState TryGetProjects(string path, IConsoleLog log, out IReadOnlyList<string> files)
    {
        if (Directory.Exists(path))
        {
            var solutions = GetSolutions(path);
            string[] projects = Directory.GetFiles(path, "*.csproj", SearchOption.TopDirectoryOnly);

            if (solutions.Count > 1)
            {
                files = [];
                return ProjectsState.MultipleSolutions;
            }
            else if (solutions.Count == 1)
            {
                files = GetProjectsFromSolution(solutions[0]);
                return ProjectsState.Ok;
            }
            else if (projects.Length > 1)
            {
                files = [];
                return ProjectsState.MultipleProjects;
            }
            else if (projects.Length == 1)
            {
                files = projects;
                return ProjectsState.Ok;
            }
        }
        else if (IsSoltuionFile(path))
        {
            files = GetProjectsFromSolution(path);
            return ProjectsState.Ok;
        }
        else if (IsCsprojectFile(path))
        {
            files = [path];
            return ProjectsState.Ok;
        }

        files = [];
        return ProjectsState.NoProjects;
    }

    private static bool IsSoltuionFile(string file)
        => File.Exists(file)
            && (Path.GetExtension(file).Equals(".sln", StringComparison.OrdinalIgnoreCase)
                || Path.GetExtension(file).Equals(".slnx", StringComparison.OrdinalIgnoreCase));

    private static bool IsCsprojectFile(string file)
        => File.Exists(file) && Path.GetExtension(file).Equals(".csproj", StringComparison.OrdinalIgnoreCase);

    private static string[] GetProjectsFromSolution(string file)
    {
        using var reader = File.OpenText(file);
        var directory = Path.GetDirectoryName(file) ?? throw new InvalidOperationException("Couldn't get solution directory");
        return SolutionFileParser.GetProjects(reader, ".csproj", directory).ToArray();
    }
}
