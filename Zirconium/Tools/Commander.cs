using Avalonia.Threading;
using System.Diagnostics;

namespace Zirconium.Tools;

public static class Commander
{
    public static (int exitCode, string stdout, string stderr) RunProcess(string fileName, string arguments, bool bypass_actionDB=false)
    {
        var result = RunProcessBlocking(fileName, arguments);
        if (bypass_actionDB)
            return result;
        ActionDatabase.RecordAction($"{fileName} {arguments}", result.ToString());
        return result;
    }


    public static (int exitCode, string stdout, string stderr) RunProcessBlocking(
        string fileName,
        string arguments)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();

        string stdout = process.StandardOutput.ReadToEnd();
        string stderr = process.StandardError.ReadToEnd();

        process.WaitForExit();

        return (process.ExitCode, stdout, stderr);
    }
}
