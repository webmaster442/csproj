using System.Runtime.CompilerServices;
using System.Xml.Linq;

using CsProj.Core;
using CsProj.Domain;

using Microsoft.Build.Construction;

namespace CsProj.Infrastructure;

internal static class Loader
{
    public static async Task<Either<LoadError, IReadOnlyList<CsharpProject>>> LoadProjectsAsync(string path,
                                                                                                ILogger logger,
                                                                                                CancellationToken cancellationToken = default)
    {
        if (Directory.Exists(path))
        {
            var solutions = GetSolutions(path);
            string[] projects = Directory.GetFiles(path, "*.csproj", SearchOption.TopDirectoryOnly);


            if (solutions.Count > 1)
            {
                return LoadError.MultipleSolutions;
            }
            else if (solutions.Count == 1)
            {
                var results = await GetProjectsFromSolutionAsync(solutions[0], logger, cancellationToken).ToListAsync();
                return results;
            }
            else if (projects.Length > 1)
            {
                return LoadError.MultipleProjects;
            }
            else if (projects.Length == 1)
            {
                var project = await LoadCsProjAsync(projects[0], cancellationToken);
                return new List<CsharpProject> { project };
            }
        }
        else if (IsSoltuionFile(path))
        {
            var results = await GetProjectsFromSolutionAsync(path, logger, cancellationToken).ToListAsync();
            return results;
        }
        else if (IsCsprojectFile(path))
        {
            var project = await LoadCsProjAsync(path, cancellationToken);
            return new List<CsharpProject> { project };
        }

        return LoadError.NoProjects;
    }

    private static IReadOnlyList<string> GetSolutions(string path)
    {
        List<string> solutions = new List<string>();
        solutions.AddRange(Directory.GetFiles(path, "*.sln", SearchOption.TopDirectoryOnly));
        solutions.AddRange(Directory.GetFiles(path, "*.slnx", SearchOption.TopDirectoryOnly));
        return solutions;
    }

    private static bool IsSoltuionFile(string file)
        => File.Exists(file)
        && (Path.GetExtension(file).Equals(".sln", StringComparison.OrdinalIgnoreCase)
            || Path.GetExtension(file).Equals(".slnx", StringComparison.OrdinalIgnoreCase));


    private static bool IsCsprojectFile(string file)
        => File.Exists(file) && Path.GetExtension(file).Equals(".csproj", StringComparison.OrdinalIgnoreCase);

    private static async IAsyncEnumerable<CsharpProject> GetProjectsFromSolutionAsync(string solutionPath,
                                                                                      ILogger logger,
                                                                                      [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var solution = SolutionFile.Parse(solutionPath);
        foreach (var project in solution.ProjectsInOrder)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                yield break;
            }
            if (project.ProjectType == SolutionProjectType.KnownToBeMSBuildFormat &&
                project.RelativePath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
            {
                if (File.Exists(project.AbsolutePath))
                {
                    yield return await LoadCsProjAsync(project.AbsolutePath, cancellationToken);
                }
                else
                {
                    logger.Warning("Project file not found: {0}", project.AbsolutePath);
                }
            }
        }
    }

    private static async Task<CsharpProject> LoadCsProjAsync(string absolutePath,
                                                              CancellationToken cancellationToken)
    {
        string xmlContent = await File.ReadAllTextAsync(absolutePath, cancellationToken);
        XDocument document = XDocument.Parse(xmlContent);
        return new CsharpProject(absolutePath, document);
    }
}