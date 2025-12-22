using System.ComponentModel;

using CsProj.Core;
using CsProj.Domain;

using Spectre.Console;
using Spectre.Console.Cli;

namespace CsProj.Commands;

internal sealed class TargetFrameworkCommand : BaseModifyCommand<TargetFrameworkCommand.Settings>
{
    internal sealed class Settings : BaseModifySettings
    {
        [Description("The target framework to upgrade to")]
        [CommandOption(CommandOptions.Target)]
        public string TargetFramework { get; set; } = string.Empty;

        [Description("The old target framework to upgrade from")]
        [CommandOption(CommandOptions.Old)]
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
