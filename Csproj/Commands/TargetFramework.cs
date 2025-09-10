using System.ComponentModel;

using Csproj.Domain;
using Csproj.DomainServices;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Csproj.Commands;

internal sealed class TargetFramework : BaseCommand<TargetFramework.Settings>
{
    protected override void ModifyProject(ProjectManipulator manipulator, Settings settings)
    {
        manipulator.SetTargetFramework(settings.TargetFramework, settings.Oldframework);
    }

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
}
