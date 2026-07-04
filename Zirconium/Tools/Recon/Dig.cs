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
        if (scan is (string target, string recordType, List<string> args))
            Scan(target, recordType, args);
        else
            throw new ArgumentException("Dig Error: Expected target string, (target, recordType), or (target, recordType, args) tuple");
    }

    public void Scan(string target, string recordType = "A", params List<string> arguments)
    {
        string cmd = $"{target} {recordType}";
        foreach (string arg in arguments)
            cmd += $" {arg}";
        var results = Commander.RunProcess(Name, cmd);
        scan_out = results.stdout;
    }
}