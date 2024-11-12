using System.ComponentModel;

using Csproj.Infrastructure;

using Spectre.Console.Cli;

namespace Csproj.Commands;

internal sealed class NullabilityCommand : BaseCommand<NullabilityCommand.Settings>
{
    internal sealed class Settings : SettingsBase
    {
        [Description("The nullability to upgrade to")]
        [CommandOption("-n|--nullability")]
        public bool Nullability { get; set; }
    }

    protected override void UpdateProject(CsprojManipulator project, Settings settings)
    {
        project.SetNullable(settings.Nullability).Save(settings.CreateBackup);
    }
}
