using System.ComponentModel;

using CsProj.Core;
using CsProj.Domain;

using Spectre.Console;
using Spectre.Console.Cli;

namespace CsProj.Commands.Modify;

internal sealed class ImplicitUsingsCommand : BaseModifyProjectCommand<ImplicitUsingsCommand.Settings>
{
    public sealed class Settings : BaseModifySettings
    {
        [Description("Enable or disable implicit usings types")]
        [CommandOption(CommandOptions.Enable)]
        public bool Enable { get; set; }
    }

    public ImplicitUsingsCommand(ILogger logger, IAnsiConsole console, TimeProvider timeProvider)
        : base(logger, console, timeProvider)
    {
    }

    protected override void ModifyProject(CsharpProject project, Settings settings)
        => project.SetImplicitUsings(settings.Enable);
}
