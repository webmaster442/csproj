using CsProj.Core;
using CsProj.Domain;
using CsProj.Infrastructure;

using Spectre.Console;
using Spectre.Console.Cli;

namespace CsProj.Commands;

internal abstract class BaseCommand<TSettings> : AsyncCommand<TSettings>
    where TSettings : BaseSettings
{
    protected readonly ILogger _logger;
    protected readonly IAnsiConsole _console;
    private readonly TimeProvider _timeProvider;

    public BaseCommand(ILogger logger, IAnsiConsole console, TimeProvider timeProvider)
    {
        _logger = logger;
        _console = console;
        _timeProvider = timeProvider;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context,
                                                    TSettings settings,
                                                    CancellationToken cancellationToken)
    {
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
                    if (settings.CreateBackup)
                    {
                        File.Move(project.AbsolutePath, Path.ChangeExtension(project.AbsolutePath, ".bak"));
                    }

                    await File.WriteAllTextAsync(project.AbsolutePath,
                                                 project.XmlContent,
                                                 cancellationToken);

                    _logger.Info("Modified project: {0}", project.AbsolutePath);
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

    protected abstract void ModifyProject(CsharpProject project, TSettings settings);
}
