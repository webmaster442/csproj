using System.Diagnostics;

namespace CsProj.Infrastructure;

internal static class GitDetector
{
    public static bool IsInsideGitRepository(string path)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "rev-parse --is-inside-work-tree",
                    WorkingDirectory = path,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
                return false;

            var output = process.StandardOutput.ReadToEnd().Trim();
            return output.Equals("true", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}
