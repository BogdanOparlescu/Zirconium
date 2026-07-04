namespace Zirconium.Tools.Recon;

public class Gobuster : ReconTool
{
    public Gobuster() : base(
        "gobuster",
        "Directory/file & DNS brute-forcing tool",
        "github.com/OJ/gobuster",
        ReconTool.Type.ActiveRecon
        )
    { }

    public override void Scan(object scan)
    {
        if (scan is (string target, string wordlist, List<string> args))
            Scan(target, wordlist, args);
        else
            throw new ArgumentException("Gobuster Error: Expected (target, wordlist) or (target, wordlist, args) tuple");
    }

    public void Scan(string target, string wordlist, params List<string> arguments)
    {
        string cmd = $"dir -u {target} -w {wordlist}";
        foreach (string arg in arguments)
            cmd += $" {arg}";
        var results = Commander.RunProcess(Name, cmd);
        scan_out = results.stdout;
    }
}