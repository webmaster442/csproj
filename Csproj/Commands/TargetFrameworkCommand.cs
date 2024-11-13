using System.ComponentModel;

using Csproj.Infrastructure;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Csproj.Commands;

internal sealed class TargetFrameworkCommand : BaseCommand<TargetFrameworkCommand.Settings>
{
    internal sealed class Settings : SettingsBase
    {
        [Description("The target framework to upgrade to")]
        [CommandOption("-t|--target")]
        public string TargetFramework { get; set; } = string.Empty;

        [Description("The old target framework to upgrade from")]
        [CommandOption("-o|--old")]
        public string Oldframework { get; set; } = string.Empty;

        public override ValidationResult Validate()
        {
            if (string.IsNullOrWhiteSpace(TargetFramework))
            {
                return ValidationResult.Error("value not set for mandatory switch -t or --target");
            }

            return base.Validate();
        }
    }

    protected override bool UpdateProject(CsprojManipulator project, Settings settings)
    {
        return project
            .SetTargetFramework(settings.TargetFramework, settings.Oldframework)
            .Save(settings.CreateBackup);
    }
}
