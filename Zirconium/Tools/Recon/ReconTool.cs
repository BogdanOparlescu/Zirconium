namespace Zirconium.Tools.Recon;

public abstract class ReconTool : Tool
{
    public string Name { get; }
    public string Description { get; }
    public string ObtainingSource { get; }

    public enum Type { PassiveRecon, ActiveRecon };
    public Type type;

    public ReconTool(string name, string description, string obtainingSource, Type type)
    {
        Name = name;
        Description = description;
        ObtainingSource = obtainingSource;
        this.type = type;
    }

    public bool VerifyInstall()
    {
        throw new NotImplementedException();
    }

    public string Version()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
