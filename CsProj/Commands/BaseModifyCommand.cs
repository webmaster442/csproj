using System.Runtime.CompilerServices;

using CsProj.Core;
using CsProj.Domain;
using CsProj.Infrastructure;

using Spectre.Console;
using Spectre.Console.Cli;

namespace CsProj.Commands;

internal abstract class BaseModifyCommand<TSettings> : AsyncCommand<TSettings>
    where TSettings : BaseModifySettings
{
    protected readonly ILogger _logger;
    protected readonly IAnsiConsole _console;
    protected readonly TimeProvider _timeProvider;

    public BaseModifyCommand(ILogger logger, IAnsiConsole console, TimeProvider timeProvider)
    {
        _logger = logger;
        _console = console;
        _timeProvider = timeProvider;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context,
                                                    TSettings settings,
                                                    CancellationToken cancellationToken)
    {
        bool isGitRepo = GitDetector.IsInsideGitRepository(settings.Path);

        (bool result, int exitCode) = ConfirmNoBackup(settings, isGitRepo);
        if (!result)
        {
            return exitCode;
        }

        Either<IReadOnlyList<CsharpProject>, LoadError> loadResult
            = await Loader.LoadProjectsAsync(settings.Path, _logger, cancellationToken);

        if (loadResult.IsFailure(out LoadError error))
        {
            _logger.Error(error);
            return ExitCodes.LoadError;
        }

        if (!loadResult.IsSuccess(out IReadOnlyList<CsharpProject>? projects)
            || projects == null)
        {
            return ExitCodes.GeneralError;
        }

        Statistics stats = new();

        try
        {
            var startTime = _timeProvider.GetUtcNow();
            foreach (var project in projects)
            {
                if (!project.IsSdkStyleProject)
                {
                    _logger.Warning("Project {Project} is not an SDK-style project. Skipping.", project.AbsolutePath);
                    stats.Skipped++;
                    continue;
                }

                _logger.Info("Porcessing file: {0}", project.AbsolutePath);

                ModifyProject(project, settings);

                if (project.WasModified)
                {
                    await SaveProject(project, settings, cancellationToken);
                    stats.Modified++;
                }
                else
                {
                    _logger.Info("No changes made: {0}", project.AbsolutePath);
                    stats.NotModified++;
                }
            }

            TimeSpan runtime = _timeProvider.GetUtcNow() - startTime;
            _logger.Info(stats, runtime);

            return ExitCodes.Success;

        }
        catch (Exception ex)
        {
            _console.WriteException(ex);
            return ExitCodes.Crash;
        }

    }

    protected (bool result, int exitCode) ConfirmNoBackup(TSettings settings, bool isGitRepo)
    {
        if (!isGitRepo && !settings.CreateBackup && !settings.Force)
        {
            _console.MarkupLine("[yellow]Warning! You are about to modify projects[/]");
            _console.MarkupLine("[yellow]The specified path is not inside a git repository and you are not backuping files[/]");
            _console.MarkupLine("[red]Without backup your projects might become corrupted[/]");
            bool confirm = _console.Confirm("Do you want to continue?", false);
            if (!confirm)
            {
                _logger.Info("Operation cancelled by user.");
                return (result: false, exitCode: ExitCodes.GeneralError);
            }
        }

        return (result: true, exitCode: default);
    }

    protected async Task SaveProject(CsharpProject project, TSettings settings, CancellationToken cancellationToken)
    {
        if (settings.CreateBackup)
        {
            File.Move(project.AbsolutePath, Path.ChangeExtension(project.AbsolutePath, ".bak"));
        }

        await File.WriteAllTextAsync(project.AbsolutePath,
                                     project.XmlContent,
                                     cancellationToken);

        _logger.Info("Modified project: {0}", project.AbsolutePath);
    }

    protected abstract void ModifyProject(CsharpProject project, TSettings settings);
}
