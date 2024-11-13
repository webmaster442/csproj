using System.Text.RegularExpressions;

namespace Csproj.Infrastructure;

internal static class ProjectProvider
{
    internal static IReadOnlyList<string> GetProjectFiles(string projectPath, string filter, bool recursive)
    {
        if (File.Exists(projectPath))
            return [projectPath];

        var results =  Directory.GetFiles(projectPath,
                                          "*.csproj",
                                          recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

        return string.IsNullOrEmpty(filter)
            ? results
            : results
            .Where(file => WildcardToRegex(filter).IsMatch(Path.GetFileName(file)))
            .ToList();
    }

    internal static Regex WildcardToRegex(string pattern)
    {
        var resultPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
        return new Regex(resultPattern, RegexOptions.Compiled);
    }
}
