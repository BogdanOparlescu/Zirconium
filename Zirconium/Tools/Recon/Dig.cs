namespace Zirconium.Tools.Recon;

public class Dig : ReconTool
{
    public Dig() : base(
        "dig",
        "DNS lookup utility",
        "sudo apt install dnsutils",
        ReconTool.Type.ActiveRecon
        )
    { }

    public override void Scan(object scan)
    {
        if (scan is (string ip, string domain))
            Scan(ip, domain);
        else
            throw new ArgumentException("Dig Error: Expected target string, (target, recordType), or (target, recordType, args) tuple");
    }

    public void Scan(string ip, string domain)
    {
        var results = Commander.RunProcess(Name, $"@{ip} {domain}");
        scan_out = results.stdout;
    }

    public override bool VerifyInstall()
    {
        try
        {
            return Commander.RunProcess(Name, "-v", true).exitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}