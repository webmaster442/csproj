using System.ComponentModel;

using Csproj.Domain;
using Csproj.DomainServices;

using Spectre.Console.Cli;

namespace Csproj.Commands;

internal sealed class Nullable : BaseCommand<Nullable.Settings>
{
    protected override void ModifyProject(ProjectManipulator manipulator, Settings settings)
    {
        manipulator.SetNullable(settings.Enable);
    }

    internal sealed class Settings : SettingsBase
    {
        [Description("Enable or disable nullable reference types")]
        [CommandOption("-e|--enable")]
        public bool Enable { get; set; }
    }
}
