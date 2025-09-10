using System.ComponentModel;

using Csproj.Domain;
using Csproj.DomainServices;

using Spectre.Console.Cli;

namespace Csproj.Commands;

internal sealed class Version : BaseCommand<Version.Settings>
{
    protected override void ModifyProject(ProjectManipulator manipulator, Settings settings)
    {
        manipulator.SetVersion(settings.Version.ToString());

        if (settings.AssemblyVersion != null)
            manipulator.SetAssemblyVersion(settings.AssemblyVersion.ToString());

        if (settings.FileVersion != null)
            manipulator.SetFileVersion(settings.FileVersion.ToString());
    }

    internal sealed class Settings : SettingsBase
    {
        [Description("The version prefix to set")]
        [CommandOption("-v|--version")]
        public System.Version Version { get; set; } = new System.Version(1, 0, 0, 0);

        [Description("The file version to set")]
        [CommandOption("-f|--file")]
        public System.Version? FileVersion { get; set; }

        [Description("The file version to set")]
        [CommandOption("-a|--assembly")]
        public System.Version? AssemblyVersion { get; set; }
    }
}