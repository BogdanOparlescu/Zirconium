namespace Zirconium.Tools.Recon;

public abstract class ReconTool : Scanner
{
    public enum Type { PassiveRecon, ActiveRecon };
    public Type type;

    public ReconTool(string name, string description, string obtainingSource, Type type) : base(name,description,obtainingSource)
    {
        this.type = type;
    }
}
