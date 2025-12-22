using System.ComponentModel;

using Spectre.Console.Cli;

namespace CsProj.Commands;

internal abstract class BaseReadSettings : CommandSettings
{
    [Description("Solution or project file name or a directory containing one")]
    [CommandArgument(0, "<path>")]
    public string Path { get; set; } = Environment.CurrentDirectory;
}
