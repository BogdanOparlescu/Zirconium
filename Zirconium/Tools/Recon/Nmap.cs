namespace Zirconium.Tools.Recon;

public class Nmap : ReconTool
{
    public Nmap() : base(
        "nmap",
        "Network Scanning Tool",
        "nmap.org",
        ReconTool.Type.ActiveRecon
        ) 
    { }

    public override void Scan(object scan)
    {
        if (scan is (string target, List<string> args))
            Scan(target, args);
        else
            throw new ArgumentException("Nmap Error");
    }

    public void Scan(string target, params List<string> arguments)
    {
        //throw new Exception("testing rout call fails!");
        string cmd = "";
        //if (!arguments.Contains("-sS"))
        //    arguments.Add("-sS");
        foreach (string arg in arguments)
            cmd += $" {arg}";
        var results = Commander.RunProcess(Name, $"{cmd} {target}");
        scan_out = results.stdout;
    }
}
