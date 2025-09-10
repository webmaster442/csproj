using Csproj.Domain.Interfaces;

namespace Csproj.DomainServices;

internal sealed class LicenseHeaderApplier
{
    private readonly Dictionary<string, string> _headers;
    private readonly IConsoleLog _consoleLog;

    public LicenseHeaderApplier(Dictionary<string, string> headers, IConsoleLog consoleLog)
    {
        _headers = headers;
        _consoleLog = consoleLog;
    }

    public void Apply(string directory, bool dryRun, bool recursive)
    {
        foreach (var header in _headers)
        {
            string[] files = Directory.GetFiles(directory, SearchPattern(header.Key), recursive ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories);
            foreach (var file in files)
            {
                ApplyToFile(file, header.Value, dryRun);
            }
        }
    }

    private void ApplyToFile(string file, string value, bool dryRun)
    {
        string originalContents = File.ReadAllText(file);
        if (!originalContents.StartsWith(value))
        {
            if (!dryRun)
            {
                using var writer = File.CreateText(file);
                writer.Write(value);
                writer.Write(originalContents);
            }
            _consoleLog.Info($"{(dryRun ? "[dryrun] " : string.Empty)}Updated {file}");
        }
    }

    private static string SearchPattern(string key)
        => key.StartsWith('.') ? $"*{key}" : key;
}
