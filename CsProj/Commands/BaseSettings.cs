using System.ComponentModel;

using Spectre.Console.Cli;

namespace CsProj.Commands;

internal abstract class BaseSettings : CommandSettings
{
    [Description("Solution or project file name or a directory containing one")]
    [CommandArgument(0, "<path>")]
    public string Path { get; set; } = Environment.CurrentDirectory;

    [Description("Create a backup of the modified project file(s)")]
    [CommandOption("-b|--backup")]
    public bool CreateBackup { get; set; }
}
