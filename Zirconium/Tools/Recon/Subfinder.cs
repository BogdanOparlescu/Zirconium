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
        throw new NotImplementedException();
    }
}
