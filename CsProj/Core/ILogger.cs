namespace CsProj.Domain;

internal interface ILogger
{
    void Error(string template, params string[] args);
    void Info(string template, params string[] args);
    void Warning(string template, params string[] args);
    void Debug(string template, params string[] args);
}