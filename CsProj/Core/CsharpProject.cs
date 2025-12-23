using System.Xml.Linq;

namespace CsProj.Core;

internal sealed class CsharpProject : IReadonlyCsharpProject
{
    private readonly XDocument _document;

    public CsharpProject(string absolutePath, XDocument document)
    {
        AbsolutePath = absolutePath;
        _document = document;
        WasModified = false;

        var sdkAttribute = document?.Element("Project")?.Attribute("Sdk");
        IsSdkStyleProject = sdkAttribute != null && !string.IsNullOrWhiteSpace(sdkAttribute.Value);
    }

    public string AbsolutePath { get; }

    public string XmlContent => _document.ToString();

    public bool WasModified { get; private set; }

    public bool IsSdkStyleProject { get; }

    public void SetNullable(bool enabled)
    {
        var value = enabled ? "enable" : "disable";
        var nullableElement = _document.Element("Project")
            ?.Element("PropertyGroup")
            ?.Element("Nullable");

        if (nullableElement == null)
        {
            _document.Element("Project")
                ?.Element("PropertyGroup")
                ?.Add(new XElement("Nullable", value));
        }
        else
        {
            nullableElement.Value = value;
        }

        WasModified = true;
    }

    public void SetTargetFramework(string targetFramework, string? oldFramework = null)
    {
        var targetFrameworkElement = _document.Element("Project")
            ?.Element("PropertyGroup")
            ?.Element("TargetFramework");

        if (targetFrameworkElement == null)
        {
            throw new InvalidOperationException("TargetFramework element not found in the project file.");
        }

        if (!string.IsNullOrEmpty(oldFramework))
        {
            if (targetFrameworkElement.Value == oldFramework)
            {
                targetFrameworkElement.Value = targetFramework;
                WasModified = true;
            }
        }
        else
        {
            targetFrameworkElement.Value = targetFramework;
            WasModified = true;
        }
    }

    public void SetVersion(string versionString)
    {
        var versionElement = _document.Element("Project")
            ?.Element("PropertyGroup")
            ?.Element("Version");

        if (versionElement == null)
        {
            _document.Element("Project")
                ?.Element("PropertyGroup")
                ?.Add(new XElement("Version", versionString));
        }
        else
        {
            versionElement.Value = versionString;
        }
        WasModified = true;
    }

    public void SetAssemblyVersion(string versionString)
    {
        var assemblyVersionElement = _document.Element("Project")
            ?.Element("PropertyGroup")
            ?.Element("AssemblyVersion");

        if (assemblyVersionElement == null)
        {
            _document.Element("Project")
                ?.Element("PropertyGroup")
                ?.Add(new XElement("AssemblyVersion", versionString));
        }
        else
        {
            assemblyVersionElement.Value = versionString;
        }

        WasModified = true;
    }

    public void SetFileVersion(string versionString)
    {
        var fileVersionElement = _document.Element("Project")
            ?.Element("PropertyGroup")
            ?.Element("FileVersion");

        if (fileVersionElement == null)
        {
            _document.Element("Project")
                ?.Element("PropertyGroup")
                ?.Add(new XElement("FileVersion", versionString));
        }
        else
        {
            fileVersionElement.Value = versionString;
        }

        WasModified = true;
    }

    public IEnumerable<PackageReference> GetPackageReferences()
    {
        var packages = _document.Descendants("PackageReference");
        foreach (var package in packages)
        {
            var includeAttribute = package.Attribute("Include");
            var versionAttribute = package.Attribute("Version");
            if (includeAttribute != null)
            {
                Version? version = null;
                if (versionAttribute != null 
                    && Version.TryParse(versionAttribute.Value, out var parsedVersion))
                {
                    version = parsedVersion;
                }
                yield return new(includeAttribute.Value, version);
            }
        }
    }

    public IEnumerable<string> GetProjectReferencesAbsolutePath()
    {
        var projects = _document.Descendants("ProjectReference");
        foreach (var project in projects)
        {
            var includeAttribute = project.Attribute("Include");
            if (includeAttribute != null)
            {
                var referencedProjectPath = Path.GetFullPath(
                    Path.Combine(Path.GetDirectoryName(AbsolutePath) ?? string.Empty, includeAttribute.Value));
  
                yield return referencedProjectPath;
            }
        }
    }

    public void RemovePackageReference(string packageName)
    {
        var packages = _document.Descendants("PackageReference")
            .Where(pr => pr.Attribute("Include")?.Value == packageName)
            .ToList();

        foreach (var package in packages)
        {
            package.Remove();
            WasModified = true;
        }
    }

    public void RemovePackageReferenceVersion(string packageName)
    {
        var packages = _document.Descendants("PackageReference")
            .Where(pr => pr.Attribute("Include")?.Value == packageName)
            .ToList();

        foreach (var package in packages)
        {
            var versionAttribute = package.Attribute("Version");
            if (versionAttribute != null)
            {
                versionAttribute.Remove();
                WasModified = true;
            }
        }
    }
}