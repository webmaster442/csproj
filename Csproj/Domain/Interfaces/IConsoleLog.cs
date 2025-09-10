using Spectre.Console;

namespace Csproj.Domain.Interfaces;

internal interface IConsoleLog
{
    void ReportProjectProcessBegin(string projectPath);
    void RerportProjectProcessEnd(string? waringMessage = null);
    void RerportProjectProcessEndNoChange();
    void Warning(string message);
    void Error(string message);
    void Info(string message);
    void ReportProgress(int count, int done);
    void ClearProgress();
}
