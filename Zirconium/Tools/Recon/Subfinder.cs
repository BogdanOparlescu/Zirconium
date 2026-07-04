namespace Zirconium.Tools.Recon;

public class Subfinder : ReconTool
{
    public Subfinder():base(
        "subfinder",
        "Passive subdomain discovery",
        "github.com/projectdiscovery/subfinder",
        ReconTool.Type.PassiveRecon
        )
    { }

    public override void Scan(object scan)
    {
        if (scan is (string domain, List<string> args))
            Scan(domain, args);
        else
            throw new ArgumentException("Subfinder Error: Expected domain string or (domain, args) tuple");
    }

    public void Scan(string domain, params List<string> arguments)
    {
        string cmd = "";
        foreach (string arg in arguments)
            cmd += $" {arg}";
        var results = Commander.RunProcess(Name, $"{cmd} -d {domain}");
        scan_out = results.stdout;
    }
}
