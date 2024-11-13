using System.ComponentModel;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Csproj;

internal abstract class SettingsBase : CommandSettings
{
    [Description("Project File Path. Can be a directory or a single project file. If not provided, the current directory is used")]
    [CommandOption("-p|--project")]
    public string ProjectPath { get; set; } = Environment.CurrentDirectory;

    [Description("Recursive search for csproj files.")]
    [CommandOption("-r|--recursive")]
    public bool Recursive { get; set; }

    [Description("Filter project files by name. Wildcards like * and ? are supported")]
    [CommandOption("-f|--filter")]
    public string Filter { get; set; } = string.Empty;

    [Description("Create a backup of the project file")]
    [CommandOption("-b|--backup")]
    public bool CreateBackup { get; set; }

    public override ValidationResult Validate()
    {
        if (File.Exists(ProjectPath))
        {
            if (Path.GetExtension(ProjectPath) == ".csproj")
            {
                return ValidationResult.Error("Specified file is not a csproj file");
            }
            else if (Recursive)
            {
                return ValidationResult.Error("Cannot specify a single project file and recursive search");
            }
        }
        else if (!Directory.Exists(ProjectPath))
        {
            return ValidationResult.Error("The specified project directory does not exist");
        }

        return ValidationResult.Success();
    }
}
