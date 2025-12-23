using System.ComponentModel;

using CsProj.Core;
using CsProj.Domain;

using Spectre.Console;
using Spectre.Console.Cli;

namespace CsProj.Commands.Modify;

internal sealed class LangVersionCommand : BaseModifyCommand<LangVersionCommand.Settings>
{
    public sealed class Settings : BaseModifySettings
    {
        [Description("Set the C# language version. Can be a major.minor number or preview, latest, latestmajor")]
        [CommandOption(CommandOptions.Version)]
        public string Version { get; set; } = string.Empty;

        public override ValidationResult Validate()
        {
            if (!Enum.TryParse<LangVersion>(Version, true, out _)
                && !System.Version.TryParse(Version, out _))
            {
                ValidationResult.Error($"'{Version}' is not a valid C# language version.");
            }
            return ValidationResult.Success();
        }
    }

    public LangVersionCommand(ILogger logger, IAnsiConsole console, TimeProvider timeProvider)
    : base(logger, console, timeProvider)
    {
    }

    protected override void ModifyProject(CsharpProject project, Settings settings)
    {
        if (Enum.TryParse<LangVersion>(settings.Version, true, out LangVersion knownVersion))
        {
            project.SetLangVersion(knownVersion);
        }
        else if (Version.TryParse(settings.Version, out Version? version))
        {
            project.SetLangVersion(version.Major, version.Minor);
        }
    }
}