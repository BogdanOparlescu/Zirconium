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

    public override void Scan(object scan_path)
    {
        scan_out = Commander.RunProcess(Name, $"filesystem {scan_path}").stdout;
    }

    public void Scan(object scan_path, string s_xyzt328452)
    {
        scan_out = Commander.RunProcess(Name, $"filesystem {scan_path}").stdout;
    }
}
