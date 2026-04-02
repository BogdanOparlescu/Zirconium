namespace Zirconium.Scanners.CodeScanners;

public class Trufflehog : CodeScanner
{
    public Trufflehog() : base(
        "trufflehog",
        "Secret Finding",
        "github.com/trufflesecurity/trufflehog",
        new List<CodeLanguage>() { CodeLanguage.C | CodeLanguage.Cpp | CodeLanguage.Python }
        )
    { }

    public string scan_out = string.Empty;
    public override void Scan(object scan_path)
    {
        scan_out = Commander.RunProcess(Name, $"filesystem {scan_path}").stdout;
    }
}
