using System.ComponentModel;

using CsProj.Core;
using CsProj.Domain;

using Spectre.Console;
using Spectre.Console.Cli;

namespace CsProj.Commands;

internal sealed class TargetFrameworkCommand : BaseCommand<TargetFrameworkCommand.Settings>
{
    internal sealed class Settings : BaseSettings
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

    public TargetFrameworkCommand(ILogger logger, IAnsiConsole console, TimeProvider timeProvider) : base(logger, console, timeProvider)
    {
    }

    protected override void ModifyProject(CsharpProject project, Settings settings)
        => project.SetTargetFramework(settings.TargetFramework, settings.Oldframework);

}
