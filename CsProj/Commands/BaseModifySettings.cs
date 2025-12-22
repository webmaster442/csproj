using System.ComponentModel;

using Spectre.Console.Cli;

namespace CsProj.Commands;

public abstract class BaseModifySettings : BaseReadSettings
{
    [Description("Create a backup of the modified project file(s)")]
    [CommandOption(CommandOptions.Backup)]
    public bool CreateBackup { get; set; }

    [Description("Force run, even if not in a git repo")]
    [CommandOption(CommandOptions.Force)]
    public bool Force { get; set; }
}
