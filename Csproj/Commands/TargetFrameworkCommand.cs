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

        public override ValidationResult Validate()
        {
            if (string.IsNullOrWhiteSpace(TargetFramework))
            {
                return ValidationResult.Error("Target Framework is required");
            }

            return base.Validate();
        }
    }

    protected override void UpdateProject(CsprojManipulator project, Settings settings)
    {
        project.SetTargetFramework(settings.TargetFramework).Save(settings.CreateBackup);
    }
}
