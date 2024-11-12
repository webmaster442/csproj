namespace Csproj.Infrastructure;

internal static class ProjectProvider
{
    internal static IReadOnlyList<string> GetProjectFiles(string projectPath, bool recursive)
    {
        if (File.Exists(projectPath))
            return [projectPath];

        return Directory.GetFiles(projectPath,
                                  "*.csproj",
                                  recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
    }
}
