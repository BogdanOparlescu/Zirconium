using System.Diagnostics;

namespace Zirconium.Scanners.CodeScanners;

public class Trufflehog : CodeScanner
{
    public Trufflehog() : base(
        "Trufflehog",
        "Secret Finding",
        "github.com/trufflesecurity/trufflehog",
        new List<CodeLanguage>() { CodeLanguage.C | CodeLanguage.Cpp | CodeLanguage.Python }
        )
    { }

    public string scan_out = string.Empty;
    public override void Scan(object scan_path)
    {
        var process = new Process();
        process.StartInfo.FileName = "trufflehog";
        process.StartInfo.Arguments = $"filesystem {scan_path}";
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;

        process.Start();

        scan_out = process.StandardOutput.ReadToEnd();
    }

    public override bool VerifyInstall()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "trufflehog",
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
