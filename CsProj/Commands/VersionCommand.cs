using System.ComponentModel;

using CsProj.Core;
using CsProj.Domain;

using Spectre.Console;
using Spectre.Console.Cli;

namespace CsProj.Commands;

internal sealed class VersionCommand : BaseModifyCommand<VersionCommand.Settings>
{
    internal sealed class Settings : BaseModifySettings
    {
        [Description("The version prefix to set")]
        [CommandOption(CommandOptions.Version)]
        public System.Version Version { get; set; } = new System.Version(1, 0, 0, 0);

        [Description("The file version to set")]
        [CommandOption(CommandOptions.FileVersion)]
        public System.Version? FileVersion { get; set; }

        [Description("The file version to set")]
        [CommandOption(CommandOptions.AssemblyVersion)]
        public System.Version? AssemblyVersion { get; set; }
    }

    public VersionCommand(ILogger logger, IAnsiConsole console, TimeProvider timeProvider)
        : base(logger, console, timeProvider)
    {
    }

    protected override void ModifyProject(CsharpProject project, Settings settings)
    {
        project.SetVersion(settings.Version.ToString());

        if (settings.AssemblyVersion != null)
            project.SetAssemblyVersion(settings.AssemblyVersion.ToString());

        if (settings.FileVersion != null)
            project.SetFileVersion(settings.FileVersion.ToString());
    }
}
