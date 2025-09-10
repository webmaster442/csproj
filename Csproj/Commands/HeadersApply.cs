using System.ComponentModel;

using Csproj.DomainServices;
using Csproj.Infrastructure;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Csproj.Commands;

internal sealed class HeadersApply : Command<HeadersApply.Settings>
{
    public class Settings : CommandSettings
    {
        [Description("Directory to process. Default: current working directory")]
        [CommandOption("-d|--directory")]
        public string Directory { get; set; }

        [Description("License template file")]
        [CommandOption("-t|--template")]
        public string TemplateFile { get; set; }

        [Description("Dryrun mode. - don't do any changes, just output what would happen.")]
        [CommandOption("-D|--dryrun")]
        public bool DryRun { get; set; }

        [Description("Recursive mode. Process subdirectories as well.")]
        [CommandOption("-r|--recursive")]
        public bool Recursive { get; set; }

        public Settings()
        {
            Directory = Environment.CurrentDirectory;
            TemplateFile = Path.Combine(Environment.CurrentDirectory, "licenses.xml");
        }

        public override ValidationResult Validate()
        {
            if (!System.IO.Directory.Exists(Directory))
                return ValidationResult.Error($"{Directory} doesn't exist");

            if (!System.IO.File.Exists(TemplateFile))
                return ValidationResult.Error($"{TemplateFile} doesn't exist");

            return ValidationResult.Success();
        }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var log = new ConsoleLog(AnsiConsole.Console);
        Dictionary<string, string> headerModels = null!;
        try
        {
            headerModels = LicenseHeaderParser.Parse(settings.TemplateFile);
        }
        catch (Exception ex)
        {
            log.Error($"Error parsing {settings.TemplateFile}: {ex.Message}");
            return ExitCodes.Error;
        }

        if (headerModels.Count == 0)
        {
            log.Error($"No headers found in {settings.TemplateFile}. Aborting.");
            return ExitCodes.Error;
        }
        var applier = new LicenseHeaderApplier(headerModels, log);

        applier.Apply(directory: settings.Directory,
                      dryRun: settings.DryRun, 
                      recursive: settings.Recursive);

        return ExitCodes.Success;
    }
}
