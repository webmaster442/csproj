using System.Xml.Linq;

namespace CsProj.Core;


internal sealed class CsharpProject
{
    private readonly XDocument _document;

    public CsharpProject(string absolutePath, XDocument document)
    {
        AbsolutePath = absolutePath;
        _document = document;
        WasModified = false;
    }

    public string AbsolutePath { get; }

    public string XmlContent => _document.ToString();

    public bool WasModified { get; private set; }

    public void SetNullable(bool enabled)
    {
        var value = enabled ? "enable" : "disable";
        var nullableElement = _document.Element("Project")?.Element("PropertyGroup")?.Element("Nullable");
        if (nullableElement == null)
        {
            _document.Element("Project")?.Element("PropertyGroup")?.Add(new XElement("Nullable", value));
        }
        else
        {
            nullableElement.Value = value;
        }

        WasModified = true;
    }

    public void SetTargetFramework(string targetFramework, string? oldFramework = null)
    {
        var targetFrameworkElement = _document.Element("Project")?.Element("PropertyGroup")?.Element("TargetFramework");
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
        var versionElement = _document.Element("Project")?.Element("PropertyGroup")?.Element("Version");
        if (versionElement == null)
        {
            _document.Element("Project")?.Element("PropertyGroup")?.Add(new XElement("Version", versionString));
        }
        else
        {
            versionElement.Value = versionString;
        }
        WasModified = true;
    }

    public void SetAssemblyVersion(string versionString)
    {
        var assemblyVersionElement = _document.Element("Project")?.Element("PropertyGroup")?.Element("AssemblyVersion");
        if (assemblyVersionElement == null)
        {
            _document.Element("Project")?.Element("PropertyGroup")?.Add(new XElement("AssemblyVersion", versionString));
        }
        else
        {
            assemblyVersionElement.Value = versionString;
        }

        WasModified = true;
    }

    public void SetFileVersion(string versionString)
    {
        var fileVersionElement = _document.Element("Project")?.Element("PropertyGroup")?.Element("FileVersion");
        if (fileVersionElement == null)
        {
            _document.Element("Project")?.Element("PropertyGroup")?.Add(new XElement("FileVersion", versionString));
        }
        else
        {
            fileVersionElement.Value = versionString;
        }

        WasModified = true;
    }


}