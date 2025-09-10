using System.ComponentModel;

using Csproj.Domain;
using Csproj.Infrastructure;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Csproj.Commands;

internal sealed class HeadersCreate : Command<HeadersCreate.Settings>
{
    public class  Settings : CommandSettings
    {
        [Description("License template file. If not specified it creates license-template.xml in current directory")]
        [CommandOption("-t|--template")]
        public string TemplateFile { get; set; }

        public Settings()
        {
            TemplateFile = Path.Combine(Environment.CurrentDirectory, DomainConstants.LicenseTemplate);
        }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var log = new ConsoleLog(AnsiConsole.Console);

        if (File.Exists(settings.TemplateFile))
        {
            log.Error($"{settings.TemplateFile} already exists. Aborting.");
            return ExitCodes.Error;
        }

        log.Info($"Created {settings.TemplateFile}. Please edit it to fit your needs.");

        return ExitCodes.Success;
    }
}
