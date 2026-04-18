namespace Zirconium.Tools;

public abstract class Scanner: Tool
{
    public string Name { get; }
    public string Description { get; }
    public string ObtainingSource { get; }
    public MemoryTable? ScanResults = null;

    public virtual void Dispose() => ScanResults?.Dispose();

    public Scanner(string name, string description, string obtainingSource)
    {
        Name = name;
        Description = description;
        ObtainingSource = obtainingSource;
    }

    public virtual string Version() => Commander.RunProcess(Name, "--version").stdout;
    public virtual bool VerifyInstall()
    {
        try
        {
            return Commander.RunProcess(Name, "--version").exitCode == 0;
        }
        catch
        {
            return false;
        }
    }
    public abstract void Scan(object scan);
}
