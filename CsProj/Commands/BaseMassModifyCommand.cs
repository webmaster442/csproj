using CsProj.Core;
using CsProj.Domain;
using CsProj.Infrastructure;

using Spectre.Console;
using Spectre.Console.Cli;

namespace CsProj.Commands;

internal abstract class BaseMassModifyCommand<TSettings> : BaseModifyCommand<TSettings>
    where TSettings : BaseModifySettings
{
    protected BaseMassModifyCommand(ILogger logger,
                                    IAnsiConsole console,
                                    TimeProvider timeProvider) : base(logger, console, timeProvider)
    {
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, TSettings settings, CancellationToken cancellationToken)
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
            var sdkprojects = projects.Where(p =>
            {
                if (!p.IsSdkStyleProject)
                {
                    _logger.Warning("Project {Project} is not an SDK-style project. Skipping.", p.AbsolutePath);
                    stats.Skipped++;
                }
                return p.IsSdkStyleProject;
            });

            ModifyProjects(sdkprojects, settings);

            foreach (CsharpProject project in sdkprojects)
            {
                if (project.WasModified)
                {
                    await SaveProject(project, settings, cancellationToken);
                    stats.Modified++;
                }
                else
                {
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

    protected abstract void ModifyProjects(IEnumerable<CsharpProject> sdkprojects, TSettings settings);
}
