using Csproj.Domain.Interfaces;

using Spectre.Console;

namespace Csproj.Infrastructure;

internal sealed class ConsoleLog : IConsoleLog
{
    private readonly IAnsiConsole _ansiConsole;
    private int _lastProgress;

    public ConsoleLog(IAnsiConsole ansiConsole)
    {
        _ansiConsole = ansiConsole;
        _lastProgress = -1;
    }

    public void ClearProgress()
    {
        _lastProgress = -1;
        _ansiConsole.Write("\e]9;4;0;0\x07");
    }

    public void Error(string message)
        => _ansiConsole.MarkupLineInterpolated($"[red]{message.EscapeMarkup()}[/]");

    public void Info(string message)
        => _ansiConsole.MarkupLineInterpolated($"[green]{message.EscapeMarkup()}[/]");

    public void ReportProgress(int count, int done)
    {
        int percent = (int)Math.Ceiling(((double)done / count) * 100.0);
        if (percent != _lastProgress)
        {
            _lastProgress = percent;
            _ansiConsole.Write($"\e]9;4;1;{percent}\x07");
        }
    }

    public void ReportProjectProcessBegin(string projectPath)
    {
        var fileName = Path.GetFileName(projectPath);
        _ansiConsole.MarkupInterpolated($"[orange3]{fileName.PadRight(70)}[/]... ");
    }

    public void RerportProjectProcessEndNoChange() 
        => _ansiConsole.MarkupLine("[green]♻️ No chnges were made[/]");

    public void RerportProjectProcessEnd(string? waringMessage = null)
    {
        if (string.IsNullOrEmpty(waringMessage))
        {
            _ansiConsole.MarkupLine("[green]✅ done[/]");
        }
        else
        {
            _ansiConsole.MarkupLine("⚠️");
            _ansiConsole.MarkupLineInterpolated($"  [yellow]{waringMessage.EscapeMarkup()}[/]");
        }
    }

    public void Warning(string message)
        => _ansiConsole.MarkupLineInterpolated($"[yellow]{message.EscapeMarkup()}[/]");
}
