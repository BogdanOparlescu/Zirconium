namespace Zirconium.Tools.CodeScanners;

public class Trufflehog : CodeScanner
{
    public Trufflehog() : base(
        "trufflehog",
        "Secret Finding",
        "github.com/trufflesecurity/trufflehog",
        new[]{ "*" }
        )
    { }

    public override void Scan(object scan_path)
    {
        scan_out = Commander.RunProcess(Name, $"filesystem {scan_path}").stdout;
    }

    public override string Version()
    {
        string version = base.Version();
        return version.Substring(version.LastIndexOf(' ') + 1);
    }
}
