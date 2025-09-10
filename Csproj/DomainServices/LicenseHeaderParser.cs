using System.Text;

namespace Csproj.DomainServices;

internal static class LicenseHeaderParser
{
    private const string extensionsPrefix = "extensions: ";

    internal static Dictionary<string, string> Parse(string licenseTemplate)
    {
        Dictionary<string, string> result = new();
        StringBuilder currentLicense = new();
        List<string> currentExtensions = new();
        using var reader = new StringReader(licenseTemplate);
        string? line;
        do
        {
            line = reader.ReadLine();
            if (line == null) break;

            if (line.StartsWith(extensionsPrefix))
            {
                if (currentLicense.Length > 0)
                {
                    string licenseText = currentLicense.ToString().TrimEnd();
                    foreach (string ext in currentExtensions)
                    {
                        result.TryAdd(ext, licenseText);
                    }
                    currentLicense.Clear();
                    currentExtensions.Clear();
                }
                currentExtensions = GetExtensions(line);
            }
            else if (string.IsNullOrWhiteSpace(line))
            {
                // skip empty lines between license blocks
                if (currentLicense.Length > 0) currentLicense.AppendLine();
            }
            else
            {
                currentLicense.AppendLine(line);
            }
        }
        while (line != null);

        if (currentExtensions.Count > 0 && currentLicense.Length > 0)
        {
            string licenseText = currentLicense.ToString().TrimEnd();
            foreach (string ext in currentExtensions)
            {
                result.TryAdd(ext, licenseText);
            }
            currentLicense.Clear();
            currentExtensions.Clear();
        }

        return result;
    }

    private static List<string> GetExtensions(string line)
    {
        return line.Replace(extensionsPrefix, "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }
}
