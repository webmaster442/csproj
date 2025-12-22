using System.ComponentModel;

using CsProj.Core;
using CsProj.Domain;

using Spectre.Console;
using Spectre.Console.Cli;

namespace CsProj.Commands;

internal sealed class NullableCommand : BaseModifyCommand<NullableCommand.Settings>
{
    public sealed class Settings : BaseModifySettings
    {
        [Description("Enable or disable nullable reference types")]
        [CommandOption(CommandOptions.Enable)]
        public bool Enable { get; set; }
    }

    public NullableCommand(ILogger logger, IAnsiConsole console, TimeProvider timeProvider)
        : base(logger, console, timeProvider)
    {
    }

    protected override void ModifyProject(CsharpProject project, Settings settings)
        => project.SetNullable(settings.Enable);
}
