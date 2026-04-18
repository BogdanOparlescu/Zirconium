namespace Zirconium.Tools.CodeScanners;

public class Trivy : CodeScanner
{
    public Trivy() : base(
        "trivy",
        "Scanner for vulnerabilities in container images, file systems, and Git repositories, as well as for configuration issues and hard-coded secrets",
        "github.com/aquasecurity/trivy/pkgs/container/trivy",
        new[]{ "*" }
        )
    { }

    public string scan_out = string.Empty;
    public override void Scan(object scan_path)
    {
        scan_out = Commander.RunProcess(Name, $"filesystem {scan_path}").stdout;
    }
}
