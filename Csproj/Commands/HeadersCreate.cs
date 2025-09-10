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
        [Description($"License template file. If not specified it creates {LicenseHeaderModel.LicenseTemplateFileName} in current directory")]
        [CommandOption("-t|--template")]
        public string TemplateFile { get; set; }

        public Settings()
        {
            TemplateFile = Path.Combine(Environment.CurrentDirectory, LicenseHeaderModel.LicenseTemplateFileName);
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

        var templateContent = """
            ﻿extensions: .cs
            //-----------------------------------------------------------------------------
            // (c) 2025 Your Name
            //-----------------------------------------------------------------------------

            extensions: .c
            /*-----------------------------------------------------------------------------
             (c) 2019-2025 Your Name
            -----------------------------------------------------------------------------*/
            """;

        File.WriteAllText(settings.TemplateFile, templateContent);
        log.Info($"Created {settings.TemplateFile}. Please edit it to fit your needs.");

        return ExitCodes.Success;
    }
}
