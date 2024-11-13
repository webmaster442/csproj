using System.ComponentModel;

using Csproj.Infrastructure;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Csproj.Commands;

internal sealed class NullabilityCommand : BaseCommand<NullabilityCommand.Settings>
{
    internal sealed class Settings : SettingsBase
    {
        [Description("The nullability to upgrade to")]
        [CommandOption("-n|--nullability")]
        public bool? Nullability { get; set; }

        public override ValidationResult Validate()
        {
            if (!Nullability.HasValue)
            {
                return ValidationResult.Error("value not set for mandatory switch -n or --nullability");
            }
            return base.Validate();
        }
    }

    protected override bool UpdateProject(CsprojManipulator project, Settings settings)
    {
        return project
            .SetNullable(settings.Nullability!.Value)
            .Save(settings.CreateBackup);
    }
}
