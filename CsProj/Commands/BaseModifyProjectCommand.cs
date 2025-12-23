using CsProj.Core;
using CsProj.Domain;

using Spectre.Console;

namespace CsProj.Commands;

internal abstract class BaseModifyProjectCommand<TSettings> : BaseModifyProjectsCommand<TSettings>
    where TSettings : BaseModifySettings
{
    protected BaseModifyProjectCommand(ILogger logger, IAnsiConsole console, TimeProvider timeProvider) 
        : base(logger, console, timeProvider)
    {
    }

    protected override bool TryModifyProjects(IEnumerable<CsharpProject> sdkprojects, TSettings settings)
    {
        foreach (var project in sdkprojects)
        {
            ModifyProject(project, settings);
        }
        return true;
    }

    protected abstract void ModifyProject(CsharpProject project, TSettings settings);
}