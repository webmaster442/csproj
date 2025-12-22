using CsProj.Core;
using CsProj.Domain;
using CsProj.Infrastructure;

using Spectre.Console;
using Spectre.Console.Cli;

namespace CsProj.Commands;

internal abstract class  BaseReadCommand<Tsettings> : AsyncCommand<Tsettings>
    where Tsettings : BaseReadSettings
{
    protected readonly ILogger _logger;
    protected readonly IAnsiConsole _console;
    private readonly TimeProvider _timeProvider;

    public BaseReadCommand(ILogger logger, IAnsiConsole console, TimeProvider timeProvider)
    {
        _logger = logger;
        _console = console;
        _timeProvider = timeProvider;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Tsettings settings, CancellationToken cancellationToken)
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

        try
        {
            var startTime = _timeProvider.GetUtcNow();
            var sdkprojects = projects.Where(p =>
            {
                if (!p.IsSdkStyleProject)
                {
                    _logger.Warning("Project {Project} is not an SDK-style project. Skipping.", p.AbsolutePath);
                }
                return p.IsSdkStyleProject;
            });
            CollectDataFromProjects(sdkprojects, settings, cancellationToken);
            TimeSpan runtime = _timeProvider.GetUtcNow() - startTime;
            _logger.Info(runtime);

            return ExitCodes.Success;

        }
        catch (Exception ex)
        {
            _console.WriteException(ex);
            return ExitCodes.Crash;
        }
    }

    protected abstract void CollectDataFromProjects(IEnumerable<IReadonlyCsharpProject> sdkprojects, Tsettings settings, CancellationToken cancellationToken);
}
