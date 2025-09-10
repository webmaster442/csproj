using System.Diagnostics;
using System.Xml.Linq;

using Csproj.Domain;
using Csproj.Domain.Interfaces;
using Csproj.DomainServices;
using Csproj.Infrastructure;

using Spectre.Console;
using Spectre.Console.Cli;

namespace Csproj;

internal abstract class BaseCommand<T> : Command<T> where T : SettingsBase
{
    public override int Execute(CommandContext context, T settings)
    {
        var log = new ConsoleLog(AnsiConsole.Console);
        var projectState = ProjectProvider.TryGetProjects(settings.Path, log, out IReadOnlyList<string> projectFiles);

        if (projectState != ProjectsState.Ok)
        {
            BaseCommand<T>.DisplayError(projectState, log);
            return ExitCodes.Error;
        }

        int done = 0;
        int modified = 0;
        int skipped = 0;
        try
        {
            var start = DateTime.UtcNow;

            foreach (var projecFile in projectFiles)
            {
                log.ReportProjectProcessBegin(projecFile);

                if (!File.Exists(projecFile))
                {
                    log.RerportProjectProcessEnd("File doesn't exist");
                    skipped++;
                    continue;
                }

                ProjectManipulator manipulator = BaseCommand<T>.CreateFromFile(projecFile);
                if (!manipulator.IsSdkStyleProject())
                {
                    log.RerportProjectProcessEnd("it is not an SDK style project");
                    skipped++;
                    continue;
                }

                ModifyProject(manipulator, settings);

                if (manipulator.WasModified)
                {
                    BaseCommand<T>.SaveProject(projecFile, manipulator.GetXml(), settings.CreateBackup);
                    log.RerportProjectProcessEnd();
                    modified++;
                }
                else
                {
                    log.RerportProjectProcessEndNoChange();
                }

                done++;
                log.ReportProgress(projectFiles.Count, done);
            }

            var end = DateTime.UtcNow;
            log.ClearProgress();

            log.Info($"Processed {projectFiles.Count} projects in {(end - start).TotalSeconds} seconds");
            if (modified > 0)
                log.Info($"Modified: {modified}");

            if (skipped > 0)
                log.Warning($"Skipped: {skipped}");

            return ExitCodes.Success;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return ExitCodes.Crash;
        }
    }

    private static void DisplayError(ProjectsState projectState, IConsoleLog log)
    {
        switch (projectState)
        {
            case ProjectsState.Ok:
                break;
            case ProjectsState.NoProjects:
                log.Warning("No projects found");
                break;
            case ProjectsState.MultipleProjects:
                log.Warning("Multiple projects found. Please specify project name explicitly");
                break;
            case ProjectsState.MultipleSolutions:
                log.Warning("Multiple solutions found. Please specify solution name explicitly");
                break;
            default:
                throw new UnreachableException("Unknown project state");
        }
    }

    private static void SaveProject(string projecFile, string contents, bool createBackup)
    {
        if (createBackup)
        {
            File.Move(projecFile, projecFile + ".bak");
        }
        File.WriteAllText(projecFile, contents);
    }

    private static ProjectManipulator CreateFromFile(string projecFile)
    {
        XDocument document = XDocument.Load(projecFile);
        return new ProjectManipulator(document, projecFile);
    }

    protected abstract void ModifyProject(ProjectManipulator manipulator, T settings);
}
