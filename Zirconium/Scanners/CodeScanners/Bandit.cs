using System.Diagnostics;

namespace Zirconium.Scanners.CodeScanners;

public class Bandit : CodeScanner
{
    public Bandit() : base(
        "Bandit",
        "Python Code Scanner",
        "pip install bandit",
        new List<CodeLanguage>() { CodeLanguage.Python }
        )
    { }

    public string scan_out = string.Empty;
    public override void Scan(object scan_path)
    {
        var process = new Process();
        process.StartInfo.FileName = "bandit";
        process.StartInfo.Arguments = $"-r {scan_path} --format custom --msg-template \"{{abspath}}:{{line}}: {{test_id}}[bandit]: {{severity}}: {{msg}}\"";
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;

        process.Start();

        scan_out = process.StandardOutput.ReadToEnd();
    }

    public override bool VerifyInstall()
    {
        //To do: bandit --version
        throw new NotImplementedException();
    }
}
