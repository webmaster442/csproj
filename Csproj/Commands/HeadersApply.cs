using System.ComponentModel;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Csproj.Commands;

internal sealed class HeadersApply : AsyncCommand<HeadersApply.Settings>
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

    public override Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        throw new NotImplementedException();
    }
}
