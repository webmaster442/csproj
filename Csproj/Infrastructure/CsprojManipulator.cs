using System.Xml.Linq;

namespace Csproj.Infrastructure;

internal class CsprojManipulator
{

    private readonly XDocument _project;
    private readonly string _projectFile;

    public CsprojManipulator(string projectFile)
    {
        _projectFile = projectFile;
        _project = XDocument.Load(projectFile);
    }

    public void Save(bool createBackup)
    {
        if (createBackup)
            File.Move(_projectFile, _projectFile + ".bak");

        // Saving this way to avoid the XML declaration being added to the file
        var xml = _project.ToString();
        File.WriteAllText(_projectFile, xml);
    }

    public CsprojManipulator SetTargetFramework(string framework)
    {
        var targetFrameworkElement = _project.Element("Project")?.Element("PropertyGroup")?.Element("TargetFramework");
        if (targetFrameworkElement == null)
        {
            throw new InvalidOperationException("TargetFramework element not found in the project file.");
        }
        targetFrameworkElement.Value = framework;
        return this;
    }

    public CsprojManipulator SetNullable(bool enabled)
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
        return this;
    }
}
