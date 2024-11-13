using System.Xml;
using System.Xml.Linq;

namespace Csproj.Infrastructure;

internal class CsprojManipulator
{

    private readonly XDocument? _project;
    private readonly string _projectFile;
    private readonly Logger _logger;

    private bool _modified;

    public CsprojManipulator(string projectFile, Logger logger)
    {
        _projectFile = projectFile;
        _logger = logger;
        try
        {
            _project = XDocument.Load(projectFile);
        }
        catch (XmlException ex)
        {
            logger.Error($"Error loading project file {projectFile}: {ex.Message}");
            _project = null;
        }
    }

    public bool Save(bool createBackup)
    {
        if (_project == null) return false;

        if (!_modified)
            return false;

        if (createBackup)
            File.Move(_projectFile, _projectFile + ".bak");

        // Saving this way to avoid the XML declaration being added to the file
        var xml = _project.ToString();
        File.WriteAllText(_projectFile, xml);

        _modified = false;

        return true;
    }

    private bool IsSdkStyleProject()
    {
        var sdkAttribute = _project?.Element("Project")?.Attribute("Sdk");
        bool sdkStyle = sdkAttribute != null && !string.IsNullOrWhiteSpace(sdkAttribute.Value);
        if (!sdkStyle)
        {
            _logger.Warning($"{_projectFile} is not an SDK style project");
        }
        return sdkStyle;
    }


    public CsprojManipulator SetTargetFramework(string targetFramework, string oldFramework)
    {
        if (_project == null || !IsSdkStyleProject()) return this;

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
                _modified = true;
            }
        }
        else
        {
            targetFrameworkElement.Value = targetFramework;
            _modified = true;
        }
        return this;
    }

    public CsprojManipulator SetNullable(bool enabled)
    {
        if (_project == null || !IsSdkStyleProject()) return this;
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
        _modified = true;
        return this;
    }
}
