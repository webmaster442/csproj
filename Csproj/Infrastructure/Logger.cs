using Spectre.Console;

namespace Csproj.Infrastructure;

internal sealed class Logger
{
    private readonly List<string> _errors;
    private readonly List<string> _warnings;

    public Logger()
    {
        _errors = new List<string>();
        _warnings = new List<string>();
    }

    public void Error(string message)
        => _errors.Add(message);

    public void Warning(string message)
        => _warnings.Add(message);

    public void DisplayLog()
    {
        if (_errors.Count > 0)
        {
            AnsiConsole.WriteLine("Errors:");
            foreach (var error in _errors)
            {
                AnsiConsole.MarkupLine($"[red]{error.EscapeMarkup()}[/]");
            }
            AnsiConsole.WriteLine();
            _errors.Clear();
        }

        if (_warnings.Count > 0)
        {
            AnsiConsole.WriteLine("Warnings:");
            foreach (var warning in _warnings)
            {
                AnsiConsole.MarkupLine($"[yellow]{warning.EscapeMarkup()}[/]");
            }
            AnsiConsole.WriteLine();
            _warnings.Clear();
        }
    }
}
