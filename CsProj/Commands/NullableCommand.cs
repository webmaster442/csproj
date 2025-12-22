using System.ComponentModel;

using CsProj.Core;
using CsProj.Domain;

using Spectre.Console;
using Spectre.Console.Cli;

namespace CsProj.Commands;

internal sealed class NullableCommand : BaseCommand<NullableCommand.Settings>
{
    internal sealed class Settings : BaseSettings
    {
        [Description("Enable or disable nullable reference types")]
        [CommandOption("-e|--enable")]
        public bool Enable { get; set; }
    }

    public NullableCommand(ILogger logger, IAnsiConsole console, TimeProvider timeProvider)
        : base(logger, console, timeProvider)
    {
    }

    protected override void ModifyProject(CsharpProject project, Settings settings)
        => project.SetNullable(settings.Enable);
}
