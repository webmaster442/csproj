using Csproj.Infrastructure;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Csproj.Commands;

internal abstract class BaseCommand<T> : Command<T> where T : SettingsBase
{
    public override int Execute(CommandContext context, T settings)
    {
        var projectFiles = ProjectProvider.GetProjectFiles(settings.ProjectPath, settings.Recursive);
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

                    var project = new CsprojManipulator(projecFile);
                    UpdateProject(project, settings);

                    task.Increment(1);
                    ctx.Refresh();
                }

            });

        return ExitCodes.Success;
    }

    protected abstract void UpdateProject(CsprojManipulator project, T settings);
}
