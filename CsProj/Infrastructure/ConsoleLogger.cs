using CsProj.Domain;

using Spectre.Console;

namespace CsProj.Infrastructure;

internal class ConsoleLogger : ILogger
{
    private readonly IAnsiConsole _console;
    private readonly TimeProvider _timeProvider;

    public ConsoleLogger(IAnsiConsole console, TimeProvider timeProvider)
    {
        _console = console;
        _timeProvider = timeProvider;
        Verbose = false;
    }

    public bool Verbose { get; set; }

    public void Debug(string template, params string[] args)
        => LogMessage("Dbg", string.Format(template, args));

    public void Error(string template, params string[] args)
        => LogMessage("Err", string.Format(template, args));

    public void Info(string template, params string[] args)
        => LogMessage("Inf", string.Format(template, args));

    public void Warning(string template, params string[] args)
        => LogMessage("Wrn", string.Format(template, args));

    private void LogMessage(string level, string message)
    {
        static string GetColorForLevel(string level) => level switch
        {
            "Err" => "red",
            "Wrn" => "yellow",
            "Inf" => "green",
            "Dbg" => "grey",
            _ => "white"
        };

        string color = GetColorForLevel(level);
        string currentTime = _timeProvider.GetLocalNow().DateTime.ToShortTimeString();
        _console.MarkupLine($"{currentTime} | {level} | [{color}]{message.EscapeMarkup()}[/]");
    }
}