using System.Xml.Linq;

namespace Csproj.DomainServices;

internal class ProjectManipulator
{
    private readonly XDocument _project;
    private readonly string _projectName;

    public bool WasModified { get; private set; }

    public ProjectManipulator(XDocument project, string projectName)
    {
        _project = project;
        _projectName = projectName;
        WasModified = false;
    }

    public bool IsSdkStyleProject()
    {
        var sdkAttribute = _project?.Element("Project")?.Attribute("Sdk");
        return sdkAttribute != null && !string.IsNullOrWhiteSpace(sdkAttribute.Value);
    }

    public ProjectManipulator SetNullable(bool enabled)
    {
        var value = enabled ? "enable" : "disable";
        var nullableElement = _project.Element("Project")?.Element("PropertyGroup")?.Element("Nullable");
        if (nullableElement == null)
        {
            _project.Element("Project")?.Element("PropertyGroup")?.Add(new XElement("Nullable", value));
        }
        else
        {
            nullableElement.Value = value;
        }

        WasModified = true;
        return this;
    }

    public ProjectManipulator SetTargetFramework(string targetFramework, string? oldFramework = null)
    {
        var targetFrameworkElement = _project.Element("Project")?.Element("PropertyGroup")?.Element("TargetFramework");
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
        return this;
    }

    public ProjectManipulator SetVersion(string versionString)
    {
        var versionElement = _project.Element("Project")?.Element("PropertyGroup")?.Element("Version");
        if (versionElement == null)
        {
            _project.Element("Project")?.Element("PropertyGroup")?.Add(new XElement("Version", versionString));
        }
        else
        {
            versionElement.Value = versionString;
        }

        WasModified = true;
        return this;
    }

    public ProjectManipulator SetAssemblyVersion(string versionString)
    {
        var assemblyVersionElement = _project.Element("Project")?.Element("PropertyGroup")?.Element("AssemblyVersion");
        if (assemblyVersionElement == null)
        {
            _project.Element("Project")?.Element("PropertyGroup")?.Add(new XElement("AssemblyVersion", versionString));
        }
        else
        {
            assemblyVersionElement.Value = versionString;
        }

        WasModified = true;
        return this;
    }

    public ProjectManipulator SetFileVersion(string versionString)
    {
        var fileVersionElement = _project.Element("Project")?.Element("PropertyGroup")?.Element("FileVersion");
        if (fileVersionElement == null)
        {
            _project.Element("Project")?.Element("PropertyGroup")?.Add(new XElement("FileVersion", versionString));
        }
        else
        {
            fileVersionElement.Value = versionString;
        }

        WasModified = true;
        return this;
    }

    public string GetXml()
        => _project.ToString();

}
