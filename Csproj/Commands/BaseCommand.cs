using Csproj.Infrastructure;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Csproj.Commands;

internal abstract class BaseCommand<T> : Command<T> where T : SettingsBase
{
    public override int Execute(CommandContext context, T settings)
    {
        var projectFiles = ProjectProvider.GetProjectFiles(settings.ProjectPath,
                                                           settings.Filter,
                                                           settings.Recursive);

        if (projectFiles.Count < 1)
        {
            AnsiConsole.MarkupLine("[red]No project files found[/]");
            return ExitCodes.NoFilesFound;
        }

        var logger = new Logger();

        var start = DateTime.UtcNow;
        int modifiedCount = 0;

        AnsiConsole.Progress()
            .AutoRefresh(false)
            .Columns(
            [
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new SpinnerColumn(Spinner.Known.Dots)
            ])
            .Start(ctx =>
            {
                var task = ctx.AddTask("[green]Updating project files[/]");
                task.MaxValue(projectFiles.Count);

                foreach (var projecFile in projectFiles)
                {
                    task.Description($"Updating {Path.GetFileName(projecFile)}");
                    ctx.Refresh();

                    var project = new CsprojManipulator(projecFile, logger);

                    if (UpdateProject(project, settings))
                        modifiedCount++;

                    task.Increment(1);
                    ctx.Refresh();
                }
            });
        var end = DateTime.UtcNow;

        AnsiConsole.MarkupLine($"Modified [green]{modifiedCount}[/] projects out of [blue]{projectFiles.Count}[/]");
        AnsiConsole.MarkupLine($"Total runtime: [green]{(end - start).TotalSeconds:0.000}[/] seconds");
        AnsiConsole.WriteLine();

        logger.DisplayLog();

        return ExitCodes.Success;
    }

    protected abstract bool UpdateProject(CsprojManipulator project, T settings);
}
